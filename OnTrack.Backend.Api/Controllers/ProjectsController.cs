using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Data;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/project")]
public class ProjectsController(ILogger<StatusesController> logger, ApplicationDbContext context)
	: ControllerBase
{
	private readonly ILogger<StatusesController> _logger = logger;
	private readonly ApplicationDbContext _context = context;

	private bool ProjectExists(ProjectId id)
	{
		return _context.Projects.Any(e => e.Id == id);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Project>> PostProject(Project project)
	{
		Status status = createStatusDto.ToDomainModel();

		_ = _context.Projects.Add(project);
		_ = await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
	{
		return await _context.Projects.ToListAsync();
	}

	[HttpGet("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Project>> GetProject(ProjectId projectId)
	{
		Project? project = await _context.Projects.FindAsync(projectId);

		return project switch
		{
			null => NotFound(),
			_ => project
		};
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutProject(Project project)
	{
		_context.Entry(project).State = EntityState.Modified;

		try
		{
			_ = await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) when (ProjectExists(project.Id) == false)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteProject(ProjectId projectId)
	{
		Project? project = await _context.Projects.FindAsync(projectId);

		if (project is null)
		{
			return NotFound();
		}

		_ = _context.Projects.Remove(project);

		// TODO Utwórz IDatabaseService i przenieś do niego tę logikę do niego
		try
		{
			_ = await _context.SaveChangesAsync();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the project with id {ProjectId}.", projectId);

			return Conflict();
		}
	}
}
