using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

using Task = OnTrack.Backend.Api.Models.Task;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/task")]
public sealed class TasksController(IEntityAccessService<Task, TaskId> tasksService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<Task, TaskId> _tasksService = tasksService;
	private readonly ILogger<StatusesController> _logger = logger;

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Task>> PostTask(TaskDto createTaskDto, [FromServices] IMapper<Task, TaskId, TaskDto> mapper)
	{
		Task task = mapper.ToNewDomainModel(createTaskDto);

		await _tasksService.Add(task);
		await _tasksService.SaveChanges();

		return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
	}

	[HttpGet("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Task>> GetTask(TaskId taskId)
	{
		Task? task = await _tasksService.Find(taskId);

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
		IEnumerable<Task> tasks = await _tasksService.GetAll();

		return tasks.ToList();
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutTask(TaskId taskId, TaskDto taskDto, [FromServices] IMapper<Task, TaskId, TaskDto> mapper)
	{
		Task? task = await _tasksService.Find(taskId);

		if (task is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(taskDto, task);

		await _tasksService.Update(task);

		try
		{
			await _tasksService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteTask(TaskId taskId)
	{
		Task? task = await _tasksService.Find(taskId);

		if (task is null)
		{
			return NotFound();
		}

		await _tasksService.Remove(task);

		try
		{
			await _tasksService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the task with id {TaskId}.", taskId);

			return Conflict();
		}
	}
}
