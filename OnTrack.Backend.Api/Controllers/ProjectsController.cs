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
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestoneExistenceValidator)
	: GenericController<ProjectId, Project, ProjectDto, ProjectsController>(logger, projectsAccessService, mapper, projectsExistenceValidator)
{
	private readonly IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> _appUserExistenceValidator = appUserExistenceValidator;
	private readonly IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> _milestoneExistenceValidator = milestoneExistenceValidator;

	private async SysTask ValidateNestedEntitiesExistence(Project project, ProjectDto projectDto)
	{
		project.Members = [];
		project.Milestones = [];

		await ValidateEntitiesExistence(projectDto.MemberIds, project.Members, _appUserExistenceValidator);

		if (projectDto.MilestoneIds is not null)
		{
			await ValidateEntitiesExistence(projectDto.MilestoneIds, project.Milestones, _milestoneExistenceValidator);
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

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<ProjectDtoWithId>> PostProject(ProjectDto projectDto, CancellationToken cancellationToken)
	{
		return (await Post(projectDto, cancellationToken)).Match<ActionResult<ProjectDtoWithId>>(
			(Project project) => CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, new ProjectDtoWithId(project, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<ProjectDtoWithId>> GetProject(ProjectId projectId, CancellationToken cancellationToken)
	{
		return (await Get(projectId, cancellationToken)).Match<ActionResult<ProjectDtoWithId>>(
			(Project project) => new ProjectDtoWithId(project, Mapper),
			(NotFound _) => NotFound(),
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
