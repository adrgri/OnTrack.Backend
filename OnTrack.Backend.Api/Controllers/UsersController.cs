using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Threading;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/user")]
public class UsersController(
	ILogger<UsersController> logger,
	IEntityAccessService<IdentitySystemObjectId, AppUser> usersAccessService,
	IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim> mapper,
	IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> usersCollectionValidator,
	UserManager<AppUser> userManager)
	: GenericController<IdentitySystemObjectId, AppUser, AppUserDtoSlim, UsersController>(logger, usersAccessService, mapper, usersCollectionValidator)
{
	private readonly UserManager<AppUser> _userManager = userManager;

	protected override Task<OneOf<AppUser, ValidationFailure>> ConvertToNewDomainModel(AppUserDtoSlim entityDto)
	{
		OneOf<AppUser, ValidationFailure> output = ModelState.IsValid switch
		{
			true => Mapper.ToNewDomainModel(entityDto),
			false => new ValidationFailure()
		};

		return SysTask.FromResult(output);
	}

	protected override async Task<OneOf<AppUser, NotFound, ValidationFailure>> ConvertToNewDomainModel(IdentitySystemObjectId entityId, AppUserDtoSlim entityDto)
	{
		return (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match<OneOf<AppUser, NotFound, ValidationFailure>>(appUser =>
		{
			Mapper.ToExistingDomainModel(entityDto, appUser);

			return ModelState.IsValid ? appUser : new ValidationFailure();
		},
		(NotFound notFound) => notFound);
	}

	[HttpGet("by/ids/{userIds}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<AppUserDtoSlimWithId>>> GetUsers([FromRoute] IdentitySystemObjectId[] userIds, CancellationToken cancellationToken)
	{
		// TODO: Dodaj tutaj jakiś max limit na ilość Id, żeby nie otworzyć API na potencjalny atak DDOS

		// TODO: Może to będzie lepszy pomysł?
		//IQueryable<AppUserDtoSlim> query = EntityAccessService.Query(cancellationToken)
		//	.Select(user => new AppUserDtoSlim() { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Bio = user.Bio })
		//	.Where(user => userIds.Contains(user.Id));

		//foreach (AppUserDtoSlim dtoSlim in query)
		//{
		//	// Walidacja
		//}

		return (await GetMany(userIds, cancellationToken)).Match<ActionResult<IEnumerable<AppUserDtoSlimWithId>>>(
			(List<AppUser> usersList) => usersList.ConvertAll(user => new AppUserDtoSlimWithId(user, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	// TODO: W zamian za to Query poniżej utwórz interfejsy do zarządzania poszczególnymi entities np "IAppUsersAccessService",
	// dodaj do niego metodę GetByEmail i inne, które będą potrzebne,
	// to samo zrób dla reszty entities
	[HttpGet("by/email/{email}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<AppUserDtoSlimWithId>> GetUserByEmail([EmailAddress] string email)
	{
		AppUser? user = await _userManager.FindByEmailAsync(email);

		return user switch
		{
			not null => new AppUserDtoSlimWithId(user, Mapper),
			null => NotFound()
		};
	}

	// TODO: Consolidate this with the methods from the projects controller
	private async Task<OneOf<AppUser, Unauthorized, Conflict, Canceled, UnexpectedException>> GetAuthorizedUser(CancellationToken cancellationToken)
	{
		AppUser? authorizedUser = await _userManager.GetUserAsync(User);

		if (authorizedUser is null)
		{
			Logger.LogError(
				new InvalidOperationException(),
				"User manager could not get authorized user based on the current claims principal {ClaimsPrincipal}. AppUser returned by the user manager is null.",
				User);

			return new Unauthorized();
		}

		return (await Get(authorizedUser.Id, cancellationToken)).Match<OneOf<AppUser, Unauthorized, Conflict, Canceled, UnexpectedException>>(
			(AppUser user) => user,
			(NotFound _) =>
			{
				Logger.LogError(
					new UnreachableException(),
					"Authorized user with Id {UserId} was found by the user manager but not found by the entity access service.",
					authorizedUser.Id);

				return new UnexpectedException();
			},
			(Conflict conflict) => conflict,
			(Canceled canceled) => canceled,
			(UnexpectedException unexpectedException) => unexpectedException);
	}

	[HttpGet("me")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AppUserDtoWithId>> GetAuthorizedUserInformation([FromServices] IMapper<IdentitySystemObjectId, AppUser, AppUserDto> fullAppUserMapper, CancellationToken cancellationToken)
	{
		return (await GetAuthorizedUser(cancellationToken)).Match<ActionResult<AppUserDtoWithId>>(
			authorizedUser => new AppUserDtoWithId(authorizedUser, fullAppUserMapper),
			(Unauthorized _) => StatusCode(StatusCodes.Status500InternalServerError),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut("me")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutUser(AppUserDtoSlim appUserDtoSlim, CancellationToken cancellationToken)
	{
		return await (await GetAuthorizedUser(cancellationToken)).Match<Task<IActionResult>>(
			async authorizedUser => (await Put(authorizedUser.Id, appUserDtoSlim, cancellationToken)).Match<IActionResult>(
				(AppUser _) => Ok(),
				(NotFound _) =>
				{
					Logger.LogError(
						new UnreachableException(),
						"Authorized user with Id {UserId} was found by the GetAuthorizedUser method but not found by the Put method.",
						authorizedUser.Id);

					return StatusCode(StatusCodes.Status500InternalServerError);
				},
				(ValidationFailure _) => ValidationProblem(ModelState),
				(Conflict _) => Conflict(),
				(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
				(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError)),
			(Unauthorized _) => SysTask.FromResult<IActionResult>(StatusCode(StatusCodes.Status500InternalServerError)),
			(Conflict _) => SysTask.FromResult<IActionResult>(Conflict()),
			(Canceled _) => SysTask.FromResult<IActionResult>(StatusCode(StatusCodes.Status499ClientClosedRequest)),
			(UnexpectedException _) => SysTask.FromResult<IActionResult>(StatusCode(StatusCodes.Status500InternalServerError)));
	}

	[HttpGet("search/{query}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<AppUserDtoSlimWithId>> Search(string query, CancellationToken cancellationToken)
	{
		try
		{
			// I used null-forgiving operator here because EF Core & SQL will handle nulls in the query appropriately
			List<AppUserDtoSlimWithId> usersMatchingSearchQuery = await EntityAccessService.Query(cancellationToken)
				.Where(user => user.UserName!.Contains(query) || user.FirstName!.Contains(query) || user.LastName!.Contains(query))
				.Select(user => new AppUserDtoSlimWithId(user, Mapper))
				.ToListAsync(cancellationToken);

			return usersMatchingSearchQuery.Count is not 0 ? Ok(usersMatchingSearchQuery) : NotFound();
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "search");

			return StatusCode(StatusCodes.Status499ClientClosedRequest);
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "search");

			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}
