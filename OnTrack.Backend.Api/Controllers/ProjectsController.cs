using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/project")]
public sealed class ProjectsController(
	IEntityAccessService<Project, ProjectId> projectsService,
	IEntityAccessService<AppUser, IdentitySystemObjectId> appUserService,
	IEntityAccessService<Milestone, MilestoneId> milestonesService,
	ILogger<StatusesController> logger)
	: ControllerBase
{
	// TODO: Rename all of those services to _[entity]AccessService
	private readonly IEntityAccessService<Project, ProjectId> _projectsService = projectsService;
	private readonly IEntityAccessService<AppUser, IdentitySystemObjectId> _appUsersService = appUserService;
	private readonly IEntityAccessService<Milestone, MilestoneId> _milestonesService = milestonesService;
	private readonly ILogger<StatusesController> _logger = logger;

	private ValidationProblemDetails CreateValidationProblemDetails()
	{
		return ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState);
	}

	// TODO: This is based on a StronglyTypedId class instead of the interface because the interface can not override the ToString method.
	private void AddValidationProblem<TEntityId>(ref ValidationProblemDetails? validationProblems, KeyValuePair<TEntityId, string[]> errorDescription)
		where TEntityId : StronglyTypedId
	{
		validationProblems ??= CreateValidationProblemDetails();
		validationProblems.Errors.Add(errorDescription.Key.ToString(), errorDescription.Value);
	}

	private static async IAsyncEnumerable<OneOf<TEntity, KeyValuePair<TEntityId, string[]>>> GetEntitiesOrGenerateErrorsIfDoesNotExist<TEntity, TEntityId>(
		IEnumerable<TEntityId> entityIds,
		IEntityAccessService<TEntity, TEntityId> entityAccessService)
		where TEntity : IEntity<TEntityId>
		where TEntityId : IStronglyTypedId
	{
		const string errorMessageTemplate = "{0} with specified Id does not exist.";

		foreach (TEntityId entityId in entityIds)
		{
			TEntity? entity = await entityAccessService.Find(entityId);

			yield return entity switch
			{
				not null => entity,
				null => new KeyValuePair<TEntityId, string[]>(entityId, [string.Format(errorMessageTemplate, typeof(TEntity).Name)])
			};
		}
	}

	private IAsyncEnumerable<OneOf<AppUser, KeyValuePair<IdentitySystemObjectId, string[]>>> GetAppUsersOrGenerateErrorsIfDoesNotExist(IEnumerable<IdentitySystemObjectId> memberIdsToFind)
	{
		return GetEntitiesOrGenerateErrorsIfDoesNotExist(memberIdsToFind, _appUsersService);
	}

	private async Task<OneOf<Project, ActionResult>> ValidateAppUsersExistance(Project project, IEnumerable<IdentitySystemObjectId> memberIdsToFind)
	{
		ValidationProblemDetails? validationProblems = null;

		await foreach (OneOf<AppUser, KeyValuePair<IdentitySystemObjectId, string[]>> oneOf in GetAppUsersOrGenerateErrorsIfDoesNotExist(memberIdsToFind))
		{
			oneOf.Switch(
				project.Members.Add,
				errorDescription => AddValidationProblem(ref validationProblems, errorDescription));
		}

		return validationProblems switch
		{
			null => project,
			not null => ValidationProblem(validationProblems)
		};
	}

	private IAsyncEnumerable<OneOf<Milestone, KeyValuePair<MilestoneId, string[]>>> GetMilestonesOrGenerateErrorsIfDoesNotExist(IEnumerable<MilestoneId> milestoneIdsToFind)
	{
		return GetEntitiesOrGenerateErrorsIfDoesNotExist(milestoneIdsToFind, _milestonesService);
	}

	private async Task<OneOf<Project, ActionResult>> ValidateMilestonesExistance(Project project, IEnumerable<MilestoneId> milestoneIdsToFind)
	{
		ValidationProblemDetails? validationProblems = null;

		await foreach (OneOf<Milestone, KeyValuePair<MilestoneId, string[]>> oneOf in GetMilestonesOrGenerateErrorsIfDoesNotExist(milestoneIdsToFind))
		{
			oneOf.Switch(milestone =>
			{
				project.Milestones ??= [];
				project.Milestones.Add(milestone);
			},
			errorDescription => AddValidationProblem(ref validationProblems, errorDescription));
		}

		return validationProblems switch
		{
			null => project,
			not null => ValidationProblem(validationProblems)
		};
	}

	private async Task<OneOf<Project, ActionResult>> ValidateNestedEntitesExistance(Project project, ProjectDto projectDto)
	{
		project.Members = [];
		project.Milestones = [];
		projectDto.MilestoneIds ??= [];

		IEnumerable<IdentitySystemObjectId> uniqueAppUserIds = projectDto.MemberIds.Distinct();
		IEnumerable<MilestoneId> uniqueMilestoneIds = projectDto.MilestoneIds.Distinct();

		OneOf<Project, ActionResult> appUsersExistanceValidationResult = await ValidateAppUsersExistance(project, uniqueAppUserIds);

		return await appUsersExistanceValidationResult.Match(
			project => ValidateMilestonesExistance(project, uniqueMilestoneIds),
			actionResult => System.Threading.Tasks.Task.FromResult<OneOf<Project, ActionResult>>(actionResult));
	}

	private async Task<ActionResult<Project>> AddProject(Project project)
	{
		await _projectsService.Add(project);
		await _projectsService.SaveChanges();

		return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Project>> PostProject(ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		Project project = mapper.ToNewDomainModel(projectDto);

		OneOf<Project, ActionResult> nestedEntitesExistanceValidationResult = await ValidateNestedEntitesExistance(project, projectDto);

		return await nestedEntitesExistanceValidationResult.Match(
			AddProject,
			actionResult => ReturnFromResult<ActionResult<Project>>(actionResult));
	}

	[HttpGet("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Project>> GetProject(ProjectId projectId)
	{
		Project? project = await _projectsService.Find(projectId);

		return project switch
		{
			null => NotFound(),
			_ => project
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
	{
		IEnumerable<Project> projects = await _projectsService.GetAll();

		return projects.ToList();
	}

	private async Task<OneOf<Project, ActionResult>> ValidateProjectExistance(ProjectId projectId)
	{
		Project? project = await _projectsService.Find(projectId);

		return project switch
		{
			not null => project,
			null => NotFound()
		};
	}

	private async Task<ActionResult> UpdateProject(Project project)
	{
		await _projectsService.Update(project);

		try
		{
			await _projectsService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	private static Task<T> ReturnFromResult<T>(T result)
	{
		return System.Threading.Tasks.Task.FromResult(result);
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> PutProject(ProjectId projectId, ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		async Task<ActionResult> PutProject(Project project)
		{
			mapper.ToExistingDomainModel(projectDto, project);

			OneOf<Project, ActionResult> nestedEntitesExistanceValidationResult = await ValidateNestedEntitesExistance(project, projectDto);

			return await nestedEntitesExistanceValidationResult.Match(
				UpdateProject,
				ReturnFromResult);
		}

		OneOf<Project, ActionResult> projectExistanceValidationResult = await ValidateProjectExistance(projectId);

		return await projectExistanceValidationResult.Match(
			PutProject,
			ReturnFromResult);
	}

	[HttpDelete("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteProject(ProjectId projectId)
	{
		Project? project = await _projectsService.Find(projectId);

		if (project is null)
		{
			return NotFound();
		}

		await _projectsService.Remove(project);

		try
		{
			await _projectsService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the project with id {ProjectId}.", projectId);

			return Conflict();
		}
	}
}
