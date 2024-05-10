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
	IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> projectExistenceValidator,
	IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> statusesExistenceValidator,
	IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> iconsExistenceValidator,
	IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> appUserExistenceValidator,
	IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> resourcesExistenceValidator,
	IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> attachmentsExistenceValidator)
	: GenericController<TaskId, Task, TaskDto, TasksController>(logger, tasksAccessService, mapper, tasksCollectionValidator)
{
	// TODO: Move all of those to a Task validator, they are not needed here for anything else than validation
	private readonly IAsyncCollectionValidator<ProjectId, OneOf<Project, EntityIdErrorsDescription<ProjectId>>> _projectExistenceValidator = projectExistenceValidator;
	private readonly IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> _statusesExistenceValidator = statusesExistenceValidator;
	private readonly IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> _iconsExistenceValidator = iconsExistenceValidator;
	private readonly IAsyncCollectionValidator<IdentitySystemObjectId, OneOf<AppUser, EntityIdErrorsDescription<IdentitySystemObjectId>>> _appUserExistenceValidator = appUserExistenceValidator;
	private readonly IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> _resourcesExistenceValidator = resourcesExistenceValidator;
	private readonly IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> _attachmentsExistenceValidator = attachmentsExistenceValidator;

	private async SysTask ValidateNestedEntitiesExistence(Task task, TaskDto taskDto)
	{
		// TODO: Don't forget to validate for recursive subtasks:
		//	1. The task can't be its own subtask
		//	2. Any of the subtasks can not contain any of the parent tasks as their subtasks
		//	   Validate the entire tasks/subtasks tree, maybe this could be done by adding task Ids to a list and then validating the subtask ids against this list
		//	   and if an id is found, then it'd be an illegal recursive assignment and thus a bad request?

		// TODO: Check for duplicate subtasks: if any of the subtasks are repeated any number of times then it's a bad request,
		//		 you can group by the subtask ids and then check if any of the groups have a count greater than 1

		task.AssignedMembers = [];
		task.AssignedResources = [];
		task.Attachments = [];
		task.Subtasks = [];

		OneOf<Project, NotFound> projectValidationResult = await ValidateEntityExistence(taskDto.ProjectId, _projectExistenceValidator);

		projectValidationResult.AssignIfSucceeded(existingProject => task.Project = existingProject);

		if (taskDto.StatusId is not null)
		{
			OneOf<Status, NotFound> iconValidationResult = await ValidateEntityExistence(taskDto.StatusId, _statusesExistenceValidator);

			iconValidationResult.AssignIfSucceeded(existingStatus => task.Status = existingStatus);
		}

		if (taskDto.IconId is not null)
		{
			OneOf<Icon, NotFound> iconValidationResult = await ValidateEntityExistence(taskDto.IconId, _iconsExistenceValidator);

			iconValidationResult.AssignIfSucceeded(existingIcon => task.Icon = existingIcon);
		}

		if (taskDto.AssignedResourceIds is not null)
		{
			await ValidateEntitiesExistence(taskDto.AssignedResourceIds, task.AssignedResources, _resourcesExistenceValidator);
		}

		if (taskDto.AssignedMemberIds is not null)
		{
			await ValidateEntitiesExistence(taskDto.AssignedMemberIds, task.AssignedMembers, _appUserExistenceValidator);
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
			(Task task) => CreatedAtAction(nameof(GetTasks), new List<object>() { new { taskId = task.Id } }, new TaskDtoWithId(task, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{taskIds}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<TaskDtoWithId>>> GetTasks([FromRoute] TaskId[] taskIds, CancellationToken cancellationToken)
	{
		return (await GetMany(taskIds, cancellationToken)).Match<ActionResult<IEnumerable<TaskDtoWithId>>>(
			(List<Task> tasksList) => tasksList.ConvertAll(task => new TaskDtoWithId(task, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
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
			(Task _) => Ok(),
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
