﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
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

	// TODO: W zamian za to Query poniżej utwórz interfejsy do zarządzania poszczególnymi entities np "IAppUsersAccessService" i dodaj do niego metodę GetByEmail i inne, które będą potrzebne,
	// to samo zrób dla reszty entities
	[HttpGet("byEmail/{email}")]
	[Authorize]
	public async Task<ActionResult<AppUserDtoWithId>> GetUserByEmail([EmailAddress] string email)
	{
		// TODO: Upewnij się, że tylko jeden użytkownik może mieć dany email i nie będzie zduplikowanych adresów email
		AppUser? user = await _userManager.FindByEmailAsync(email);

		return user switch
		{
			not null => new AppUserDtoWithId(user, Mapper),
			null => NotFound()
		};
	}

	private async Task<OneOf<AppUser, Error>> GetAuthorizedUser()
	{
		AppUser? authorizedUser = await _userManager.GetUserAsync(User);

		if (authorizedUser is null)
		{
			Logger.LogError(new UnreachableException(),
				   "User manager could not get authorized user based on the current claims principal {ClaimsPrincipal}. Authorized user is null.",
				   User);

			return new Error();
		}

		return authorizedUser;
	}

	[HttpGet]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<AppUserDtoWithId>> GetUser()
	{
		return (await GetAuthorizedUser()).Match<ActionResult<AppUserDtoWithId>>(
			authorizedUser => new AppUserDtoWithId(authorizedUser, Mapper),
			(Error _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

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
