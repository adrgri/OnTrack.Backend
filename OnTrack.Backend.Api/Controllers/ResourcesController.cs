using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Threading;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

[ApiController, Route("/api/resource")]
public sealed class ResourcesController(
	ILogger<ResourcesController> logger,
	IEntityAccessService<ResourceId, Resource> resourcesService,
	IMapper<ResourceId, Resource, ResourceDto> mapper,
	IAsyncCollectionValidator<ResourceId, OneOf<Resource, EntityIdErrorsDescription<ResourceId>>> resourceExistenceValidator)
	: GenericController<ResourceId, Resource, ResourceDto, ResourcesController>(logger, resourcesService, mapper, resourceExistenceValidator)
{
	protected override Task<OneOf<Resource, ValidationFailure>> ConvertToNewDomainModel(ResourceDto entityDto)
	{
		Resource resource = Mapper.ToNewDomainModel(entityDto);

		OneOf<Resource, ValidationFailure> output = ModelState.IsValid ? resource : new ValidationFailure();

		return SysTask.FromResult(output);
	}

	protected override async Task<OneOf<Resource, NotFound, ValidationFailure>> ConvertToNewDomainModel(ResourceId entityId, ResourceDto entityDto)
	{
		return (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match(resource =>
		{
			Mapper.ToExistingDomainModel(entityDto, resource);

			return (OneOf<Resource, NotFound, ValidationFailure>)(ModelState.IsValid ? resource : new ValidationFailure());
		},
		(NotFound notFound) => notFound);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<ResourceDtoWithId>> PostResource(ResourceDto resourceDto, CancellationToken cancellationToken)
	{
		return (await Post(resourceDto, cancellationToken)).Match<ActionResult<ResourceDtoWithId>>(
			(Resource resource) => CreatedAtAction(nameof(GetResources), new List<object>() { new { resourceId = resource.Id } }, new ResourceDtoWithId(resource, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{resourceIds}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<ResourceDtoWithId>>> GetResources([FromRoute] ResourceId[] resourceIds, CancellationToken cancellationToken)
	{
		return (await GetMany(resourceIds, cancellationToken)).Match<ActionResult<IEnumerable<ResourceDtoWithId>>>(
			(List<Resource> resourcesList) => resourcesList.ConvertAll(resource => new ResourceDtoWithId(resource, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<ResourceDtoWithId>>> GetResources(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<ResourceDtoWithId>>>(
			(List<Resource> resourcesList) => resourcesList.ConvertAll(resource => new ResourceDtoWithId(resource, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutResource(ResourceDtoWithId resourceDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(resourceDtoWithId.Id, resourceDtoWithId, cancellationToken)).Match(
			(Resource _) => Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{resourceId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteResource(ResourceId resourceId, CancellationToken cancellationToken)
	{
		return (await Delete(resourceId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
