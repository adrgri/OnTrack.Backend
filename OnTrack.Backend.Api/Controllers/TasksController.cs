using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Task = OnTrack.Backend.Api.Models.Task;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/task")]
public class TasksController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool TaskExists(TaskId id)
	{
		return _context.Tasks.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Task>> PostTask(CreateTaskDto createTaskDto)
	{
		Task task = createTaskDto.ToDomainModel();

		_ = _context.Tasks.Add(task);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
	{
		return await _context.Tasks.ToListAsync();
	}

	[HttpGet("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Task>> GetTask(TaskId taskId)
	{
		Task? task = await _context.Tasks.FindAsync(taskId);

		return task switch
		{
			null => NotFound(),
			_ => task
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutTask(Task task)
	{
		_context.Entry(task).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (TaskExists(task.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{taskId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteTask(TaskId taskId)
	{
		Task? task = await _context.Tasks.FindAsync(taskId);

		if (task is null)
		{
			return NotFound();
		}

		_ = _context.Tasks.Remove(task);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the task with id {TaskId}.", taskId);

			return Conflict();
		}
	}
}
