using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("api/project")]
public sealed class ProjectsController(IEntityAccessService<Project, ProjectId> projectsService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<Project, ProjectId> _projectsService = projectsService;
	private readonly ILogger<StatusesController> _logger = logger;

	private async Task<OneOf<Project, ActionResult>> Validate(Project project)
	{
		//project.Members ??= [];

		//foreach (IdentitySystemObjectId userId in projectDto.MemberIds)
		//{
		//	AppUser? user = await _userManager.FindByIdAsync(userId.ToString());

		//	if (user is null)
		//	{
		//		return ValidationProblem(detail: $"User with id {userId} does not exist.", statusCode: StatusCodes.Status400BadRequest, title: "Validation failure.");
		//	}

		//	if (project.Members.Any(user => user.Id == userId) == false)
		//	{
		//		project.Members.Add(user);
		//	}
		//}

		throw new NotImplementedException();
	}

	private async Task<ActionResult<Project>> AddProject(Project project)
	{
		await _projectsService.Add(project);
		await _projectsService.SaveChanges();

		return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
	}

	private Task<ActionResult<Project>> ReturnValidationFailure(ActionResult actionResult)
	{
		return System.Threading.Tasks.Task.FromResult<ActionResult<Project>>(actionResult);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Project>> PostProject(ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		Project project = mapper.ToNewDomainModel(projectDto);

		OneOf<Project, ActionResult> validationResult = await Validate(project);

		return await validationResult.Match(AddProject, ReturnValidationFailure);
	}

	[HttpGet("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Project>> GetProject(ProjectId projectId)
	{
		Project? project = await _projectsService.Find(projectId);

		return project switch
		{
			null => NotFound(),
			_ => project
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
	{
		IEnumerable<Project> projects = await _projectsService.GetAll();

		return projects.ToList();
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutProject(ProjectId projectId, ProjectDto projectDto, [FromServices] IMapper<Project, ProjectId, ProjectDto> mapper)
	{
		Project? project = await _projectsService.Find(projectId);

		if (project is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(projectDto, project);

		await _projectsService.Update(project);

		try
		{
			await _projectsService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{projectId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteProject(ProjectId projectId)
	{
		Project? project = await _projectsService.Find(projectId);

		if (project is null)
		{
			return NotFound();
		}

		await _projectsService.Remove(project);

		try
		{
			await _projectsService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the project with id {ProjectId}.", projectId);

			return Conflict();
		}
	}
}
