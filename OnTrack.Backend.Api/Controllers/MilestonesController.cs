using Microsoft.AspNetCore.Mvc;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.OneOf;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Threading;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/milestone")]
public sealed class MilestonesController(
	ILogger<MilestonesController> logger,
	IEntityAccessService<MilestoneId, Milestone> milestonesAccessService,
	IMapper<MilestoneId, Milestone, MilestoneDto> mapper,
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestonesExistenceValidator,
	IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> projectsExistenceValidator,
	IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> statusesExistenceValidator,
	IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> tasksExistenceValidator)
	: GenericController<MilestoneId, Milestone, MilestoneDto, MilestonesController>(logger, milestonesAccessService, mapper, milestonesExistenceValidator)
{
	private readonly IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> _projectsExistenceValidator = projectsExistenceValidator;
	private readonly IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> _statusesExistenceValidator = statusesExistenceValidator;
	private readonly IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> _tasksExistenceValidator = tasksExistenceValidator;

	private async SysTask ValidateNestedEntitiesExistence(Milestone milestone, MilestoneDto milestoneDto)
	{
		milestone.Status = null;
		milestone.Tasks = [];

		OneOf<Project, NotFound> projectValidationResult = await ValidateEntityExistence(milestoneDto.ProjectId, _projectsExistenceValidator);

		projectValidationResult.AssignIfSucceeded(existingProject => milestone.Project = existingProject);

		if (milestoneDto.StatusId is not null)
		{
			OneOf<Status, NotFound> statusValidationResult = await ValidateEntityExistence(milestoneDto.StatusId, _statusesExistenceValidator);

			statusValidationResult.AssignIfSucceeded(existingStatus => milestone.Status = existingStatus);
		}

		if (milestoneDto.TaskIds is not null)
		{
			await ValidateEntitiesExistence(milestoneDto.TaskIds, milestone.Tasks, _tasksExistenceValidator);
		}
	}

	protected override async Task<OneOf<Milestone, ValidationFailure>> ConvertToNewDomainModel(MilestoneDto entityDto)
	{
		Milestone milestone = Mapper.ToNewDomainModel(entityDto);

		await ValidateNestedEntitiesExistence(milestone, entityDto);

		return ModelState.IsValid ? milestone : new ValidationFailure();
	}

	protected override async Task<OneOf<Milestone, NotFound, ValidationFailure>> ConvertToNewDomainModel(MilestoneId entityId, MilestoneDto entityDto)
	{
		return await (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match(async milestone =>
		{
			Mapper.ToExistingDomainModel(entityDto, milestone);

			await ValidateNestedEntitiesExistence(milestone, entityDto);

			return ModelState.IsValid ? milestone : new ValidationFailure();
		},
		(NotFound notFound) => SysTask.FromResult<OneOf<Milestone, NotFound, ValidationFailure>>(notFound));
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<MilestoneDtoWithId>> PostMilestone(MilestoneDto milestoneDto, CancellationToken cancellationToken)
	{
		return (await Post(milestoneDto, cancellationToken)).Match<ActionResult<MilestoneDtoWithId>>(
			(Milestone milestone) => CreatedAtAction(nameof(GetMilestone), new { milestoneId = milestone.Id }, milestone),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<MilestoneDtoWithId>> GetMilestone(MilestoneId milestoneId, CancellationToken cancellationToken)
	{
		return (await Get(milestoneId, cancellationToken)).Match<ActionResult<MilestoneDtoWithId>>(
			(Milestone milestone) => new MilestoneDtoWithId(milestone, Mapper),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<MilestoneDtoWithId>>> GetMilestones(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<MilestoneDtoWithId>>>(
			(List<Milestone> milestonesList) => milestonesList.ConvertAll(milestone => new MilestoneDtoWithId(milestone, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutMilestone(MilestoneDtoWithId milestoneDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(milestoneDtoWithId.Id, milestoneDtoWithId, cancellationToken)).Match(
			(Milestone _) => Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteMilestone(MilestoneId milestoneId, CancellationToken cancellationToken)
	{
		return (await Delete(milestoneId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
