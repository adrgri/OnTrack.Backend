using Microsoft.AspNetCore.Mvc;

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

[ApiController, Route("/api/status")]
public sealed class StatusesController(
	ILogger<StatusesController> logger,
	IEntityAccessService<StatusId, Status> statusesService,
	IMapper<StatusId, Status, StatusDto> mapper,
	IAsyncCollectionValidator<StatusId, OneOf<Status, EntityIdErrorsDescription<StatusId>>> statusExistenceValidator)
	: GenericController<StatusId, Status, StatusDto, StatusesController>(logger, statusesService, mapper, statusExistenceValidator)
{
	protected override Task<OneOf<Status, ValidationFailure>> ConvertToNewDomainModel(StatusDto entityDto)
	{
		Status status = Mapper.ToNewDomainModel(entityDto);

		OneOf<Status, ValidationFailure> output = ModelState.IsValid ? status : new ValidationFailure();

		return SysTask.FromResult(output);
	}

	protected override async Task<OneOf<Status, NotFound, ValidationFailure>> ConvertToNewDomainModel(StatusId entityId, StatusDto entityDto)
	{
		return (await ValidateEntityExistence(entityId, EntityCollectionValidator)).Match(status =>
		{
			Mapper.ToExistingDomainModel(entityDto, status);

			return (OneOf<Status, NotFound, ValidationFailure>)(ModelState.IsValid ? status : new ValidationFailure());
		},
		(NotFound notFound) => notFound);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<StatusDtoWithId>> PostStatus(StatusDto statusDto, CancellationToken cancellationToken)
	{
		return (await Post(statusDto, cancellationToken)).Match<ActionResult<StatusDtoWithId>>(
			(Status status) => CreatedAtAction(nameof(GetStatuses), new List<object>() { new { statusId = status.Id } }, new StatusDtoWithId(status, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet("{statusIds}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<StatusDtoWithId>>> GetStatuses([FromRoute] StatusId[] statusIds, CancellationToken cancellationToken)
	{
		return (await GetMany(statusIds, cancellationToken)).Match<ActionResult<IEnumerable<StatusDtoWithId>>>(
			(List<Status> statusesList) => statusesList.ConvertAll(status => new StatusDtoWithId(status, Mapper)),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<ActionResult<IEnumerable<StatusDtoWithId>>> GetStatuses(CancellationToken cancellationToken)
	{
		return (await GetAll(cancellationToken)).Match<ActionResult<IEnumerable<StatusDtoWithId>>>(
			(List<Status> statusesList) => statusesList.ConvertAll(status => new StatusDtoWithId(status, Mapper)),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> PutStatus(StatusDtoWithId statusDtoWithId, CancellationToken cancellationToken)
	{
		return (await Put(statusDtoWithId.Id, statusDtoWithId, cancellationToken)).Match(
			(Status _) => Ok(),
			(NotFound _) => NotFound(),
			(ValidationFailure _) => ValidationProblem(ModelState),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}

	[HttpDelete("{statusId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
	public async Task<IActionResult> DeleteStatus(StatusId statusId, CancellationToken cancellationToken)
	{
		return (await Delete(statusId, cancellationToken)).Match(
			(Success _) => Ok(),
			(NotFound _) => NotFound(),
			(Conflict _) => Conflict(),
			(Canceled _) => StatusCode(StatusCodes.Status499ClientClosedRequest),
			(UnexpectedException _) => StatusCode(StatusCodes.Status500InternalServerError));
	}
}
