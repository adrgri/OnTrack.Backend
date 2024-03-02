using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/status")]
public sealed class StatusesController(IEntityAccessService<StatusId, Status> statusesService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<StatusId, Status> _statusesService = statusesService;
	private readonly ILogger<StatusesController> _logger = logger;

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Status>> PostStatus(StatusDto statusDto, [FromServices] IMapper<StatusId, Status, StatusDto> mapper)
	{
		Status status = mapper.ToNewDomainModel(statusDto);

		await _statusesService.Add(status);
		await _statusesService.SaveChanges();

		return CreatedAtAction(nameof(GetStatus), new { statusId = status.Id }, status);
	}

	[HttpGet("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Status>> GetStatus(StatusId statusId)
	{
		Status? status = await _statusesService.Find(statusId);

		return status switch
		{
			null => NotFound(),
			_ => status
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
	{
		IEnumerable<Status> statuses = await _statusesService.GetAll();

		return statuses.ToList();
	}

	[HttpPut("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> PutStatus(StatusId statusId, StatusDto statusDto, [FromServices] IMapper<StatusId, Status, StatusDto> mapper)
	{
		Status? status = await _statusesService.Find(statusId);

		if (status is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(statusDto, status);

		await _statusesService.Update(status);

		try
		{
			await _statusesService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return Conflict();
		}

		return Ok();
	}

	[HttpDelete("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteStatus(StatusId statusId)
	{
		Status? status = await _statusesService.Find(statusId);

		if (status is null)
		{
			return NotFound();
		}

		await _statusesService.Remove(status);

		// TODO Przenieś tę logikę do data access service gdy już zaimplementujesz zwracanie rezultatu operacji
		try
		{
			await _statusesService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the status with id {StatusId}.", statusId);

			return Conflict();
		}
	}
}
