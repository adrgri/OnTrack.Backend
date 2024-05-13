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

[ApiController, Route("/api/project")]
public sealed class ProjectsController(
	ILogger<ProjectsController> logger,
	IEntityAccessService<ProjectId, Project> projectsAccessService,
	IMapper<ProjectId, Project, ProjectDto> mapper,
	IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> projectsExistenceValidator,
	IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> appUserExistenceValidator,
	IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> taskExistenceValidator,
	UserManager<AppUser> userManager)
	: GenericController<ProjectId, Project, ProjectDto, ProjectsController>(logger, projectsAccessService, mapper, projectsExistenceValidator)
{
	private readonly IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> _appUserExistenceValidator = appUserExistenceValidator;
	private readonly IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> _taskExistenceValidator = taskExistenceValidator;

	private readonly UserManager<AppUser> _userManager = userManager;

	private async SysTask ValidateNestedEntitiesExistence(Project project, ProjectDto projectDto)
	{
		project.Members = [];
		project.Tasks = [];

		if (projectDto.MemberIds?.Count is null or 0)
		{
			throw new UnreachableException(
				"This exception should never be thrown since at this point members should be specified or the calling code should have assigned at least the currently logged in user as a member of this project.",
				new InvalidOperationException($"The \"{projectDto.MemberIds}\" collection was null or empty."));
		}
		else
		{
		await ValidateEntitiesExistence(projectDto.MemberIds, project.Members, _appUserExistenceValidator);
		}

		if (projectDto.TaskIds is not null)
		{
			await ValidateEntitiesExistence(projectDto.TaskIds, project.Tasks, _taskExistenceValidator);
		}
	}

	protected override async Task<OneOf<Project, ValidationFailure>> ConvertToNewDomainModel(ProjectDto entityDto)
	{
		Project project = Mapper.ToNewDomainModel(entityDto);

		await ValidateNestedEntitiesExistence(project, entityDto);

		return ModelState.IsValid ? project : new ValidationFailure();
	}

	protected override async Task<OneOf<Project, NotFound, ValidationFailure>> ConvertToNewDomainModel(ProjectId entityId, ProjectDto entityDto)
	{
		return await (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match(async project =>
		{
			Mapper.ToExistingDomainModel(entityDto, project);

			await ValidateNestedEntitiesExistence(project, entityDto);

			return ModelState.IsValid ? project : new ValidationFailure();
		},
		(NotFound notFound) => SysTask.FromResult<OneOf<Project, NotFound, ValidationFailure>>(notFound));
	}

	private async Task<OneOf<AppUser, Unauthorized>> GetAuthorizedUser()
	{
		AppUser? maybeAuthorizedUser = await _userManager.GetUserAsync(User);

		if (maybeAuthorizedUser is null)
		{
			Logger.LogError(new UnreachableException(),
				"User manager could not get authorized user based on the current claims principal {ClaimsPrincipal}. Authorized user is null.",
				User);

			return new Unauthorized();
		}

		return maybeAuthorizedUser;
	}

	private async Task<OneOf<Success, Unauthorized>> EnsureAuthorizedUserIdIsPresentInTheMembersList(ProjectDto projectDto)
	{
		projectDto.MemberIds ??= [];

		return (await GetAuthorizedUser()).Match<OneOf<Success, Unauthorized>>(
			authorizedUser =>
			{
				if (projectDto.MemberIds.Contains(authorizedUser.Id) == false)
				{
					projectDto.MemberIds.Add(authorizedUser.Id);
				}

				return new Success();
			},
			(Unauthorized unauthorized) => unauthorized);
	}
	[HttpPost]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<ProjectDtoWithId>> PostProject(ProjectDto projectDto, CancellationToken cancellationToken)
	{
		return await (await EnsureAuthorizedUserIdIsPresentInTheMembersList(projectDto)).Match<Task<ActionResult<ProjectDtoWithId>>>(
			async (Success _) => (await Post(projectDto, cancellationToken)).Match<ActionResult<ProjectDtoWithId>>(
			(Project project) => CreatedAtAction(nameof(GetProjects), new List<object>() { new { projectId = project.Id } }, new ProjectDtoWithId(project, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
				(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError)),
			(Unauthorized _) => SysTask.FromResult<ActionResult<ProjectDtoWithId>>(StatusCode(StatusCodes.Status500InternalServerError)));
	}

	[HttpGet("{projectIds}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<ProjectDtoWithId>>> GetProjects([FromRoute] ProjectId[] projectIds, CancellationToken cancellationToken)
	{
		return (await GetMany(projectIds, cancellationToken)).Match<ActionResult<IEnumerable<ProjectDtoWithId>>>(
			(List<Project> projectList) => projectList.ConvertAll(project => new ProjectDtoWithId(project, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<ProjectDtoWithId>>> GetProjects(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<ProjectDtoWithId>>>(
			(List<Project> projectsList) => projectsList.ConvertAll(project => new ProjectDtoWithId(project, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutProject(ProjectDtoWithId projectDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(projectDtoWithId.Id, projectDtoWithId, cancellationToken)).Match(
			(Project _) => Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteProject(ProjectId projectId, CancellationToken cancellationToken)
	{
		return (await Delete(projectId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
