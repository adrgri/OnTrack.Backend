using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/resource")]
public class ResourcesController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool ResourceExists(ResourceId id)
	{
		return _context.Resources.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Resource>> PostResource(CreateResourceDto createResourceDto)
	{
		Resource resource = createResourceDto.ToDomainModel();

		_ = _context.Resources.Add(resource);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetResource), new { resourceId = resource.Id }, resource);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
	{
		return await _context.Resources.ToListAsync();
	}

	[HttpGet("{resourceId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Resource>> GetResource(ResourceId resourceId)
	{
		Resource? resource = await _context.Resources.FindAsync(resourceId);

		return resource switch
		{
			null => NotFound(),
			_ => resource
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutResource(Resource resource)
	{
		_context.Entry(resource).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (ResourceExists(resource.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{resourceId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteResource(ResourceId resourceId)
	{
		Resource? resource = await _context.Resources.FindAsync(resourceId);

		if (resource is null)
		{
			return NotFound();
		}

		_ = _context.Resources.Remove(resource);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the resource with id {ResourceId}.", resourceId);

			return Conflict();
		}
	}
}
