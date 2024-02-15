using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/milestone")]
public class MilestonesController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool MilestoneExists(MilestoneId id)
	{
		return _context.Milestones.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Milestone>> PostMilestone(Milestone milestone)
	{
		Status status = createStatusDto.ToDomainModel();

		_ = _context.Milestones.Add(milestone);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetMilestone), new { milestoneId = milestone.Id }, milestone);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Milestone>>> GetMilestones()
	{
		return await _context.Milestones.ToListAsync();
	}

	[HttpGet("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Milestone>> GetMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await _context.Milestones.FindAsync(milestoneId);

		return milestone switch
		{
			null => NotFound(),
			_ => milestone
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutMilestone(Milestone milestone)
	{
		_context.Entry(milestone).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (MilestoneExists(milestone.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await _context.Milestones.FindAsync(milestoneId);
	
		if (milestone is null)
		{
			return NotFound();
		}

		_ = _context.Milestones.Remove(milestone);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the milestone with id {MilestoneId}.", milestoneId);

			return Conflict();
		}
	}
}
