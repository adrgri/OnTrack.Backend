using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
	IMapper<IdentitySystemObjectId, AppUser, AppUserDto> mapper,
	IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> usersCollectionValidator,
	UserManager<AppUser> userManager)
	: GenericController<IdentitySystemObjectId, AppUser, AppUserDto, UsersController>(logger, usersAccessService, mapper, usersCollectionValidator)
{
	private readonly UserManager<AppUser> _userManager = userManager;

	protected override Task<OneOf<AppUser, ValidationFailure>> ConvertToNewDomainModel(AppUserDto entityDto)
	{
		OneOf<AppUser, ValidationFailure> output = ModelState.IsValid switch
		{
			true => Mapper.ToNewDomainModel(entityDto),
			false => new ValidationFailure()
		};

		return SysTask.FromResult(output);
	}

	protected override async Task<OneOf<AppUser, NotFound, ValidationFailure>> ConvertToNewDomainModel(IdentitySystemObjectId entityId, AppUserDto entityDto)
	{
		return (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match<OneOf<AppUser, NotFound, ValidationFailure>>(language =>
		{
			Mapper.ToExistingDomainModel(entityDto, language);

			return ModelState.IsValid ? language : new ValidationFailure();
		},
		(NotFound notFound) => notFound);
	}

	[HttpGet("by/ids/{userIds}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<AppUserDtoSlim>>> GetUsers([FromRoute] IdentitySystemObjectId[] userIds, [FromServices] IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim> mapper, CancellationToken cancellationToken)
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

		return (await GetMany(userIds, cancellationToken)).Match<ActionResult<IEnumerable<AppUserDtoSlim>>>(
			(List<AppUser> usersList) => usersList.ConvertAll(user => new AppUserDtoSlim(user, mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	// TODO: W zamian za to Query poniżej utwórz interfejsy do zarządzania poszczególnymi entities np "IAppUsersAccessService" i dodaj do niego metodę GetByEmail i inne, które będą potrzebne,
	// to samo zrób dla reszty entities
	[HttpGet("by/email/{email}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<AppUserDtoSlim>> GetUserByEmail([EmailAddress] string email, [FromServices] IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim> mapper)
	{
		AppUser? maybeUser = await _userManager.FindByEmailAsync(email);

		return maybeUser switch
		{
			not null => new AppUserDtoSlim(maybeUser, mapper),
			null => NotFound()
		};
	}

	private async Task<OneOf<AppUser, Error>> GetAuthorizedUser()
	{
		AppUser? maybeAuthorizedUser = await _userManager.GetUserAsync(User);

		if (maybeAuthorizedUser is null)
		{
			Logger.LogError(new UnreachableException(),
				   "User manager could not get authorized user based on the current claims principal {ClaimsPrincipal}. Authorized user is null.",
				   User);

			return new Error();
		}

		return maybeAuthorizedUser;
	}

	[HttpGet("me")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AppUserDtoWithId>> GetAuthorizedUserInformation()
	{
		return (await GetAuthorizedUser()).Match<ActionResult<AppUserDtoWithId>>(
			authorizedUser => new AppUserDtoWithId(authorizedUser, Mapper),
			(Error _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	//public record class AppUserPutRequestDto : IDto
	//{
	//	[ProtectedPersonalData]
	//	public string? UserName { get; set; }

	//	[EmailAddress]
	//	[ProtectedPersonalData]
	//	public string? Email { get; set; }

	//	[Length(2, 20)]
	//	[ProtectedPersonalData]
	//	public string? FirstName { get; set; }

	//	[Length(0, 40)]
	//	[ProtectedPersonalData]
	//	public string? LastName { get; set; }

	//	[Length(0, 1_000)]
	//	[ProtectedPersonalData]
	//	public string? Bio { get; set; }

	//	[ProtectedPersonalData]
	//	public Language? Language { get; set; }

	//	//public PathString? ProfilePicturePath { get; set; }
	//}

	//[HttpPut("me")]
	//[Authorize]
	//[ProducesResponseType(StatusCodes.Status200OK)]
	//[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
	//[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	//[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	//public async Task<IActionResult> PutUser(AppUserPutRequestDto appUserPutRequestDto, [FromServices] IMapper<IdentitySystemObjectId, AppUser, AppUserPutRequestDto> mapper, CancellationToken cancellationToken)
	//{
	//	return (await GetAuthorizedUser()).Match<IActionResult>(
	//		authorizedUser => mapper.ToExistingDomainModel(appUserPutRequestDto, authorizedUser),
	//		(Unauthorized _) => StatusCode(StatusCodes.Status500InternalServerError));

	//	return (await Put(appUserPutRequestDto.Id, appUserPutRequestDto, cancellationToken)).Match(
	//		(Task _) => Ok(),
	//		(NotFound _) => NotFound(),
	//		(ValidationFailure _) => ValidationProblem(ModelState),
	//		(Conflict _) => Conflict(),
	//		(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
	//		(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	//}

	//// TODO: Dodaj nowy DTO dla tego endpointa z możliwością zmiany danych użytkownika, które nie są częścią systemu tożsamościowego
	//[HttpPut]
	//[Authorize]
	//[ProducesResponseType(StatusCodes.Status200OK)]
	//[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	//[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	//public async Task<IActionResult> PutUser(AppUserDtoWithId userDtoWithId, CancellationToken cancellationToken)
	//{
	//	return (await Put(userDtoWithId.Id, userDtoWithId, cancellationToken)).Match(
	//		(AppUser _) => Ok(),
	//		(NotFound _) => NotFound(),
	//		(ValidationFailure _) => ValidationProblem(ModelState),
	//		(Conflict _) => Conflict(),
	//		(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
	//		(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	//}
}
