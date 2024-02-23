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

[ApiController, Route("/api/task")]
public sealed class TasksController(
	ILogger<TasksController> logger,
	IEntityAccessService<Task, TaskId> tasksService,
	IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> tasksExistanceValidator,
	IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> milestoneExistanceValidator,
	IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> iconsExistanceValidator,
	IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> resourcesExistanceValidator,
	IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> attachmentsExistanceValidator)
	: GenericController<Task, TaskId, TaskDto, TasksController>(logger, tasksService)
{
	private readonly IAsyncCollectionValidator<TaskId, OneOf<Task, EntityIdErrorsDescription<TaskId>>> _tasksExistanceValidator = tasksExistanceValidator;
	private readonly IAsyncCollectionValidator<MilestoneId, OneOf<Milestone, EntityIdErrorsDescription<MilestoneId>>> _milestoneExistanceValidator = milestoneExistanceValidator;
	private readonly IAsyncCollectionValidator<IconId, OneOf<Icon, EntityIdErrorsDescription<IconId>>> _iconsExistanceValidator = iconsExistanceValidator;
	private readonly IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> _resourcesExistanceValidator = resourcesExistanceValidator;
	private readonly IAsyncCollectionValidator<AttachmentId, OneOf<Attachment, EntityIdErrorsDescription<AttachmentId>>> _attachmentsExistanceValidator = attachmentsExistanceValidator;

	private async SysTask ValidateNestedEntitesExistance(Task task, TaskDto taskDto)
	{
		task.AssignedResources = [];
		task.Attachments = [];
		task.Subtasks = [];

		OneOf<Milestone, Error> milestoneValidationResult = await ValidateEntityExistance(taskDto.MilestoneId, _milestoneExistanceValidator);

		milestoneValidationResult.AssignIfSucceeded(existingMilestone => task.Milestone = existingMilestone);

		if (taskDto.IconId is not null)
		{
			OneOf<Icon, Error> iconValidationResult = await ValidateEntityExistance(taskDto.IconId, _iconsExistanceValidator);

			iconValidationResult.AssignIfSucceeded(existingIcon => task.Icon = existingIcon);
		}

		if (taskDto.AssignedResourceIds is not null)
		{
			await ValidateEntitiesExistance(taskDto.AssignedResourceIds, task.AssignedResources, _resourcesExistanceValidator);
		}

		if (taskDto.AttachmentIds is not null)
		{
			await ValidateEntitiesExistance(taskDto.AttachmentIds, task.Attachments, _attachmentsExistanceValidator);
		}

		if (taskDto.SubtaskIds is not null)
		{
			await ValidateEntitiesExistance(taskDto.SubtaskIds, task.Subtasks, _tasksExistanceValidator);
		}
	}

	private async Task<ActionResult<Task>> AddTask(Task task)
	{
		await EntityAccessService.Add(task);
		await EntityAccessService.SaveChanges();

		return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Task>> PostTask(TaskDto taskDto, [FromServices] IMapper<Task, TaskId, TaskDto> mapper)
	{
		Task task = mapper.ToNewDomainModel(taskDto);

		await ValidateNestedEntitesExistance(task, taskDto);

		return ModelState.IsValid ? await AddTask(task) : ValidationProblem(ModelState);
	}

	[HttpGet("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Task>> GetTask(TaskId taskId)
	{
		Task? task = await EntityAccessService.Find(taskId);

		return task switch
		{
			null => NotFound(),
			_ => task
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
	{
		IEnumerable<Task> tasks = await EntityAccessService.GetAll();

		return tasks.ToList();
	}

	private async Task<ActionResult> UpdateTask(Task task)
	{
		await EntityAccessService.Update(task);

		try
		{
			await EntityAccessService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, "Concurrency exception occurred while trying to delete the task with id {TaskId}.", task.Id);

			return Conflict();
		}

		return Ok();
	}

	private async Task<ActionResult> PutExistingTask(Task existingTask, TaskDto taskDto, [FromServices] IMapper<Task, TaskId, TaskDto> mapper)
	{
		mapper.ToExistingDomainModel(taskDto, existingTask);

		await ValidateNestedEntitesExistance(existingTask, taskDto);

		return ModelState.IsValid ? await UpdateTask(existingTask) : ValidationProblem(ModelState);
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutTask(TaskId taskId, TaskDto taskDto, [FromServices] IMapper<Task, TaskId, TaskDto> mapper)
	{
		OneOf<Task, Error> validationResult = await ValidateEntityExistance(taskId, _tasksExistanceValidator);

		return await validationResult.Match(
			task => PutExistingTask(task, taskDto, mapper),
			_ => SysTask.FromResult(ValidationProblem(ModelState)));
	}

	[HttpDelete("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteTask(TaskId taskId)
	{
		Task? task = await EntityAccessService.Find(taskId);

		if (task is null)
		{
			return NotFound();
		}

		await EntityAccessService.Remove(task);

		try
		{
			await EntityAccessService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, "Concurrency exception occurred while trying to delete the task with id {TaskId}.", taskId);

			return Conflict();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected exception occurred in {ActionName} endpoint.", nameof(DeleteTask));

			throw;
		}
	}
}
