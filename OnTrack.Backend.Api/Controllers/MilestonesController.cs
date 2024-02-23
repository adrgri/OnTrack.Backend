using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.OneOf;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/milestone")]
public sealed class MilestonesController(
	ILogger<MilestonesController> logger,
	IEntityAccessService<Milestone, MilestoneId> milestonesService,
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestonesExistanceValidator,
	IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> projectsExistanceValidator,
	IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> statusesExistanceValidator,
	IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> tasksExistanceValidator)
	: GenericController<Milestone, MilestoneId, MilestoneDto, MilestonesController>(logger, milestonesService)
{
	private readonly IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> _milestonesExistanceValidator = milestonesExistanceValidator;
	private readonly IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> _projectsExistanceValidator = projectsExistanceValidator;
	private readonly IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> _statusesExistanceValidator = statusesExistanceValidator;
	private readonly IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> _tasksExistanceValidator = tasksExistanceValidator;

	private async SysTask ValidateNestedEntitesExistance(Milestone milestone, MilestoneDto milestoneDto)
	{
		milestone.Status = null;
		milestone.Tasks = [];

		OneOf<Project, Error> projectValidationResult = await ValidateEntityExistance(milestoneDto.ProjectId, _projectsExistanceValidator);

		projectValidationResult.AssignIfSucceeded(existingProject => milestone.Project = existingProject);

		if (milestoneDto.StatusId is not null)
		{
			OneOf<Status, Error> statusValidationResult = await ValidateEntityExistance(milestoneDto.StatusId, _statusesExistanceValidator);

			statusValidationResult.AssignIfSucceeded(existingStatus => milestone.Status = existingStatus);
		}

		if (milestoneDto.TaskIds is not null)
		{
			await ValidateEntitiesExistance(milestoneDto.TaskIds, milestone.Tasks, _tasksExistanceValidator);
		}
	}

	private async Task<ActionResult<Milestone>> AddMilestone(Milestone milestone)
	{
		await EntityAccessService.Add(milestone);
		await EntityAccessService.SaveChanges();

		return CreatedAtAction(nameof(GetMilestone), new { milestoneId = milestone.Id }, milestone);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Milestone>> PostMilestone(MilestoneDto milestoneDto, [FromServices] IMapper<Milestone, MilestoneId, MilestoneDto> mapper)
	{
		Milestone milestone = mapper.ToNewDomainModel(milestoneDto);

		await ValidateNestedEntitesExistance(milestone, milestoneDto);

		return ModelState.IsValid ? await AddMilestone(milestone) : ValidationProblem(ModelState);
	}

	[HttpGet("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Milestone>> GetMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await EntityAccessService.Find(milestoneId);

		return milestone switch
		{
			null => NotFound(),
			_ => milestone
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Milestone>>> GetMilestones()
	{
		IEnumerable<Milestone> milestones = await EntityAccessService.GetAll();

		return milestones.ToList();
	}

	private async Task<ActionResult> UpdateMilestone(Milestone project)
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

	private async Task<ActionResult> PutExistingMilestone(Milestone existingMilestone, MilestoneDto milestoneDto, [FromServices] IMapper<Milestone, MilestoneId, MilestoneDto> mapper)
	{
		mapper.ToExistingDomainModel(milestoneDto, existingMilestone);

		await ValidateNestedEntitesExistance(existingMilestone, milestoneDto);

		return ModelState.IsValid ? await UpdateMilestone(existingMilestone) : ValidationProblem(ModelState);
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> PutMilestone(MilestoneId milestoneId, MilestoneDto milestoneDto, [FromServices] IMapper<Milestone, MilestoneId, MilestoneDto> mapper)
	{
		OneOf<Milestone, Error> validationResult = await ValidateEntityExistance(milestoneId, _milestonesExistanceValidator);

		return await validationResult.Match(
			milestone => PutExistingMilestone(milestone, milestoneDto, mapper),
			_ => SysTask.FromResult(ValidationProblem(ModelState)));
	}

	private const string _concurrencyErrorMessageTemplate = "Concurrency exception occurred while trying to {Action} the milestone with id {MilestoneId}.";

	[HttpDelete("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await EntityAccessService.Find(milestoneId);

		if (milestone is null)
		{
			return NotFound();
		}

		await EntityAccessService.Remove(milestone);

		try
		{
			await EntityAccessService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, _concurrencyErrorMessageTemplate, "delete", milestoneId);

			return Conflict();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected exception occurred in {ActionName} endpoint.", nameof(DeleteMilestone));

			throw;
		}
	}
}
