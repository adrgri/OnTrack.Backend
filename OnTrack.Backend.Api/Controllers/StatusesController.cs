using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/status")]
public sealed class StatusesController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool StatusExists(StatusId id)
	{
		return _context.Statuses.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Status>> PostStatus(CreateStatusDto createStatusDto)
	{
		Status status = createStatusDto.ToDomainModel();

		_ = _context.Statuses.Add(status);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetStatus), new { statusId = status.Id }, status);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Status>>> GetAllStatuses()
	{
		return await _context.Statuses.ToListAsync();
	}

	[HttpGet("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Status>> GetStatus(StatusId statusId)
	{
		Status? status = await _context.Statuses.FindAsync(statusId);

		return status switch
		{
			null => NotFound(),
			_ => status
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutStatus(Status status)
	{
		_context.Entry(status).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (StatusExists(status.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteStatus(StatusId statusId)
	{
		Status? status = await _context.Statuses.FindAsync(statusId);

		if (status is null)
		{
			return NotFound();
		}

		_ = _context.Statuses.Remove(status);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the status with id {StatusId}.", statusId);

			return Conflict();
		}
	}
}
