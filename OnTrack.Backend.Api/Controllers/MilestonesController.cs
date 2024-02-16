using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/milestone")]
public sealed class MilestonesController(IEntityAccessService<Milestone, MilestoneId> milestonesService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<Milestone, MilestoneId> _milestonesService = milestonesService;
	private readonly ILogger<StatusesController> _logger = logger;

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Milestone>> PostMilestone(
		MilestoneDto createMilestoneDto,
		[FromServices] IMapper<Milestone, MilestoneId, MilestoneDto> mapper,
		IEntityAccessService<Project, ProjectId> projectsService,
		IEntityAccessService<Status, StatusId> statusesService)
	{
		Milestone milestone = mapper.ToNewDomainModel(createMilestoneDto);

		Project? project = await projectsService.Find(createMilestoneDto.ProjectId);

		if (project is null)
		{
			return ValidationProblem(detail: $"Project with id {createMilestoneDto.ProjectId} does not exist.", statusCode: StatusCodes.Status400BadRequest, title: "Validation failure.");
		}

		milestone.Project = project;

		if (createMilestoneDto.StatusId is not null)
		{
			Status? status = await statusesService.Find(createMilestoneDto.StatusId);

			if (status is null)
			{
				return ValidationProblem(detail: $"Status with id {createMilestoneDto.StatusId} does not exist.", statusCode: StatusCodes.Status400BadRequest, title: "Validation failure.");
			}

			milestone.Status = status;
		}

		await _milestonesService.Add(milestone);
		await _milestonesService.SaveChanges();

		return CreatedAtAction(nameof(GetMilestone), new { milestoneId = milestone.Id }, milestone);
	}

	[HttpGet("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Milestone>> GetMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await _milestonesService.Find(milestoneId);

		return milestone switch
		{
			null => NotFound(),
			_ => milestone
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Milestone>>> GetMilestones()
	{
		IEnumerable<Milestone> milestones = await _milestonesService.GetAll();

		return milestones.ToList();
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutMilestone(MilestoneId milestoneId, MilestoneDto milestoneDto, [FromServices] IMapper<Milestone, MilestoneId, MilestoneDto> mapper)
	{
		Milestone? milestone = await _milestonesService.Find(milestoneId);

		if (milestone is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(milestoneDto, milestone);

		await _milestonesService.Update(milestone);

		try
		{
			await _milestonesService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{milestoneId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteMilestone(MilestoneId milestoneId)
	{
		Milestone? milestone = await _milestonesService.Find(milestoneId);

		if (milestone is null)
		{
			return NotFound();
		}

		await _milestonesService.Remove(milestone);

		try
		{
			await _milestonesService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the milestone with id {MilestoneId}.", milestoneId);

			return Conflict();
		}
	}
}
