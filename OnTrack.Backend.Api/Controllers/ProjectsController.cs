using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/project")]
public sealed class ProjectsController(
	ILogger<ProjectsController> logger,
	IEntityAccessService<Project, ProjectId> projectsService,
	IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> projectsExistanceValidator,
	IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> appUserExistanceValidator,
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestoneExistanceValidator)
	: GenericController<Project, ProjectId, ProjectDto, ProjectsController>(logger, projectsService)
{
	private readonly IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> _projectsExistanceValidator = projectsExistanceValidator;
	private readonly IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> _appUserExistanceValidator = appUserExistanceValidator;
	private readonly IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> _milestoneExistanceValidator = milestoneExistanceValidator;

	private async SysTask ValidateNestedEntitesExistance(Project project, ProjectDto projectDto)
	{
		project.Members = [];
		project.Milestones = [];

		await ValidateEntitiesExistance(projectDto.MemberIds, project.Members, _appUserExistanceValidator);

		if (projectDto.MilestoneIds is not null)
		{
			await ValidateEntitiesExistance(projectDto.MilestoneIds, project.Milestones, _milestoneExistanceValidator);
		}
	}

	private async Task<ActionResult<Project>> AddProject(Project project)
	{
		await EntityAccessService.Add(project);
		await EntityAccessService.SaveChanges();

		return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Project>> PostProject(ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		Project project = mapper.ToNewDomainModel(projectDto);

		await ValidateNestedEntitesExistance(project, projectDto);

		return ModelState.IsValid ? await AddProject(project) : ValidationProblem(ModelState);
	}

	[HttpGet("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Project>> GetProject(ProjectId projectId)
	{
		Project? project = await EntityAccessService.Find(projectId);

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
		IEnumerable<Project> projects = await EntityAccessService.GetAll();

		return projects.ToList();
	}

	private async Task<ActionResult> UpdateProject(Project project)
	{
		await EntityAccessService.Update(project);

		try
		{
			await EntityAccessService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, "Concurrency exception occurred while trying to delete the project with id {ProjectId}.", project.Id);

			return Conflict();
		}

		return Ok();
	}

	private async Task<ActionResult> PutExistingProject(Project existingProject, ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		mapper.ToExistingDomainModel(projectDto, existingProject);

		await ValidateNestedEntitesExistance(existingProject, projectDto);

		return ModelState.IsValid ? await UpdateProject(existingProject) : ValidationProblem(ModelState);
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> PutProject(ProjectId projectId, ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		OneOf<Project, Error> validationResult = await ValidateEntityExistance(projectId, _projectsExistanceValidator);

		return await validationResult.Match(
			project => PutExistingProject(project, projectDto, mapper),
			_ => SysTask.FromResult(ValidationProblem(ModelState)));
	}

	[HttpDelete("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteProject(ProjectId projectId)
	{
		Project? project = await EntityAccessService.Find(projectId);

		if (project is null)
		{
			return NotFound();
		}

		await EntityAccessService.Remove(project);

		try
		{
			await EntityAccessService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, "Concurrency exception occurred while trying to delete the project with id {ProjectId}.", projectId);

			return Conflict();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected exception occurred in {ActionName} endpoint.", nameof(DeleteProject));

			throw;
		}
	}
}
