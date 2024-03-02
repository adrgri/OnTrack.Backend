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

[ApiController, Route("/api/task")]
public sealed class TasksController(
	ILogger<TasksController> logger,
	IEntityAccessService<TaskId, Task> tasksAccessService,
	IMapper<TaskId, Task, TaskDto> mapper,
	IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> tasksCollectionValidator,
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestoneExistenceValidator,
	IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> iconsExistenceValidator,
	IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> resourcesExistenceValidator,
	IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> attachmentsExistenceValidator)
	: GenericController<TaskId, Task, TaskDto, TasksController>(logger, tasksAccessService, mapper, tasksCollectionValidator)
{
	private readonly IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> _milestoneExistenceValidator = milestoneExistenceValidator;
	private readonly IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> _iconsExistenceValidator = iconsExistenceValidator;
	private readonly IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> _resourcesExistenceValidator = resourcesExistenceValidator;
	private readonly IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> _attachmentsExistenceValidator = attachmentsExistenceValidator;

	private async SysTask ValidateNestedEntitiesExistence(Task task, TaskDto taskDto)
	{
		task.AssignedResources = [];
		task.Attachments = [];
		task.Subtasks = [];

		OneOf<Milestone, NotFound> milestoneValidationResult = await ValidateEntityExistence(taskDto.MilestoneId, _milestoneExistenceValidator);

		milestoneValidationResult.AssignIfSucceeded(existingMilestone => task.Milestone = existingMilestone);

		if (taskDto.IconId is not null)
		{
			OneOf<Icon, NotFound> iconValidationResult = await ValidateEntityExistence(taskDto.IconId, _iconsExistenceValidator);

			iconValidationResult.AssignIfSucceeded(existingIcon => task.Icon = existingIcon);
		}

		if (taskDto.AssignedResourceIds is not null)
		{
			await ValidateEntitiesExistence(taskDto.AssignedResourceIds, task.AssignedResources, _resourcesExistenceValidator);
		}

		if (taskDto.AttachmentIds is not null)
		{
			await ValidateEntitiesExistence(taskDto.AttachmentIds, task.Attachments, _attachmentsExistenceValidator);
		}

		if (taskDto.SubtaskIds is not null)
		{
			await ValidateEntitiesExistence(taskDto.SubtaskIds, task.Subtasks, EntityCollectionValidator);
		}
	}

	protected override async Task<OneOf<Task, ValidationFailure>> ConvertToNewDomainModel(TaskDto entityDto)
	{
		Task task = Mapper.ToNewDomainModel(entityDto);

		await ValidateNestedEntitiesExistence(task, entityDto);

		return ModelState.IsValid ? task : new ValidationFailure();
	}

	protected override async Task<OneOf<Task, NotFound, ValidationFailure>> ConvertToNewDomainModel(TaskId entityId, TaskDto entityDto)
	{
		return await (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match(async task =>
		{
			Mapper.ToExistingDomainModel(entityDto, task);

			await ValidateNestedEntitiesExistence(task, entityDto);

			return ModelState.IsValid ? task : new ValidationFailure();
		},
		(NotFound notFound) => SysTask.FromResult<OneOf<Task, NotFound, ValidationFailure>>(notFound));
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<TaskDtoWithId>> PostTask(TaskDto taskDto, CancellationToken cancellationToken)
	{
		return (await Post(taskDto, cancellationToken)).Match<ActionResult<TaskDtoWithId>>(
			(Task task) => CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<TaskDtoWithId>> GetTask(TaskId taskId, CancellationToken cancellationToken)
	{
		return (await Get(taskId, cancellationToken)).Match<ActionResult<TaskDtoWithId>>(
			(Task task) => new TaskDtoWithId(task, Mapper),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<TaskDtoWithId>>> GetTasks(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<TaskDtoWithId>>>(
			(List<Task> tasksList) => tasksList.ConvertAll(task => new TaskDtoWithId(task, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutTask(TaskDtoWithId taskDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(taskDtoWithId.Id, taskDtoWithId, cancellationToken)).Match(
			(Task _)=> Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteTask(TaskId taskId, CancellationToken cancellationToken)
	{
		return (await Delete(taskId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
