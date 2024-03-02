using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/resource")]
public sealed class ResourcesController(IEntityAccessService<ResourceId, Resource> resourcesService, ILogger<StatusesController> logger)
	: ControllerBase
{
	private readonly IEntityAccessService<ResourceId, Resource> _resourcesService = resourcesService;
	private readonly ILogger<StatusesController> _logger = logger;

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Resource>> PostResource(ResourceDto createResourceDto, [FromServices] IMapper<ResourceId, Resource, ResourceDto> mapper)
	{
		Resource resource = mapper.ToNewDomainModel(createResourceDto);

		await _resourcesService.Add(resource);
		await _resourcesService.SaveChanges();

		return CreatedAtAction(nameof(GetResource), new { resourceId = resource.Id }, resource);
	}

	[HttpGet("{resourceId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Resource>> GetResource(ResourceId resourceId)
	{
		Resource? resource = await _resourcesService.Find(resourceId);

		return resource switch
		{
			null => NotFound(),
			_ => resource
		};
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
	{
		IEnumerable<Resource> resources = await _resourcesService.GetAll();

		return resources.ToList();
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PutResource(ResourceId resourceId, ResourceDto resourceDto, [FromServices] IMapper<ResourceId, Resource, ResourceDto> mapper)
	{
		Resource? resource = await _resourcesService.Find(resourceId);

		if (resource is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(resourceDto, resource);

		await _resourcesService.Update(resource);

		try
		{
			await _resourcesService.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			return NotFound();
		}

		return Ok();
	}

	[HttpDelete("{resourceId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> DeleteResource(ResourceId resourceId)
	{
		Resource? resource = await _resourcesService.Find(resourceId);

		if (resource is null)
		{
			return NotFound();
		}

		await _resourcesService.Remove(resource);

		try
		{
			await _resourcesService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogError(ex, "Concurrency exception occurred while trying to delete the resource with id {ResourceId}.", resourceId);

			return Conflict();
		}
	}
}
