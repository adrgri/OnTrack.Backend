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

// TODO: Add a constructor parameter to give this class an ability to validate the entities by taking an async validator
// + add virtual methods to 1. prepare them for validation 2. handle and post validation results, maybe adding to the ModelState can be done in the base class
// as virtual implementation and the derived class can override it to add more specific error handling should they need it
public abstract class GenericController<TEntity, TEntityId, TDto, TController>(ILogger<TController> logger, IEntityAccessService<TEntity, TEntityId> entityAccessService)
	: ControllerBase
	where TEntity : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
	where TDto : IDto
	where TController : ControllerBase
{
	protected const string concurrencyErrorMessageLoggerTemplate = "Concurrency exception occurred while trying to {Action} the {EntityName} with id {EntityId}.";
	protected const string unexpectedExceptionMessageLoggerTemplate = "Unexpected exception occurred while trying to {Action} the {EntityName} with id {EntityId}.";
	protected static readonly string entityName = typeof(TEntity).Name;

	protected ILogger<TController> Logger { get; } = logger;
	protected IEntityAccessService<TEntity, TEntityId> EntityAccessService { get; } = entityAccessService;

	protected void LogConcurrencyException(DbUpdateConcurrencyException ex, string action, TEntityId entityId)
	{
		Logger.LogError(ex, concurrencyErrorMessageLoggerTemplate, action, entityName, entityId);
	}

	protected void LogUnexpectedException(Exception ex, string action, TEntityId entityId)
	{
		Logger.LogError(ex, unexpectedExceptionMessageLoggerTemplate, action, entityName, entityId);
	}

	protected Error RegisterEntityIdErrors<TArbitraryEntityId>(EntityIdErrorsDescription<TArbitraryEntityId> entityIdErrorsDescription)
		where TArbitraryEntityId : IStronglyTypedId
	{
		foreach (string errorDescription in entityIdErrorsDescription.ErrorsDescription)
		{
			string entityIdAsString = entityIdErrorsDescription.EntityId.ToString()
				?? throw new NullReferenceException();

			ModelState.AddModelError(entityIdAsString, errorDescription);
		}

		return new Error();
	}

	protected async Task<OneOf<TArbitraryEntity, Error>> ValidateEntityExistance<TArbitraryEntity, TArbitraryEntityId>(TArbitraryEntityId entityId, IAsyncValidator<TArbitraryEntityId, OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>>> entityExistanceValidator)
		where TArbitraryEntity : IEntity<TArbitraryEntityId>
		where TArbitraryEntityId : IStronglyTypedId
	{
		OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>> validationResult = await entityExistanceValidator.Validate(entityId);

		return validationResult.Match<OneOf<TArbitraryEntity, Error>>(
			existingEntity => existingEntity,
			entityIdErrorsDescription => RegisterEntityIdErrors(entityIdErrorsDescription));
	}

	protected async SysTask ValidateEntitiesExistance<TArbitraryEntity, TArbitraryEntityId>(ICollection<TArbitraryEntityId> entityIdsToValidate, ICollection<TArbitraryEntity> existingEntities, IAsyncCollectionValidator<TArbitraryEntityId, OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>>> entitiesExistanceValidator)
		where TArbitraryEntity : IEntity<TArbitraryEntityId>
		where TArbitraryEntityId : IStronglyTypedId
	{
		await foreach (OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>> validationResult in entitiesExistanceValidator.Validate(entityIdsToValidate))
		{
			validationResult.Switch(
				existingEntities.Add,
				entityIdErrorsDescription => RegisterEntityIdErrors(entityIdErrorsDescription));
		}
	}

	protected async Task<ActionResult<TEntity>> Post(TDto entityDto, IMapper<TEntity, TEntityId, TDto> mapper)
	{
		TEntity entity = mapper.ToNewDomainModel(entityDto);

		//if (ModelState.IsValid == false)
		//{
		//	return ValidationProblem(ModelState);
		//}

		await EntityAccessService.Add(entity);
		await EntityAccessService.SaveChanges();

		return CreatedAtAction(nameof(Get), new { languageId = entity.Id }, entity);
	}

	protected async Task<ActionResult<TEntity>> Get(TEntityId entityId)
	{
		TEntity? entity = await EntityAccessService.Find(entityId);

		return entity switch
		{
			null => NotFound(),
			_ => entity
		};
	}

	protected async Task<ActionResult<IEnumerable<TEntity>>> Get()
	{
		IEnumerable<TEntity> entities = await EntityAccessService.GetAll();

		return entities.ToList();
	}

	protected async Task<IActionResult> Put(TEntityId entityId, TDto entityDto, IMapper<TEntity, TEntityId, TDto> mapper)
	{
		TEntity? entity = await EntityAccessService.Find(entityId);

		if (entity is null)
		{
			return NotFound();
		}

		mapper.ToExistingDomainModel(entityDto, entity);

		await EntityAccessService.Update(entity);

		try
		{
			await EntityAccessService.SaveChanges();

			return Ok();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			Logger.LogError(ex, concurrencyErrorMessageLoggerTemplate, "update", entityName, entityId);

			return Conflict();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, unexpectedExceptionMessageLoggerTemplate, "delete", entityName, entityId);

			throw;
		}
	}

	protected async Task<OneOf<Success, NotFound, Conflict, Canceled, UnexpectedException>> Delete(TEntityId entityId, CancellationToken cancellationToken)
	{
		try
		{
			TEntity? entity = await EntityAccessService.Find(entityId, cancellationToken);

			if (entity is null)
			{
				return new NotFound();
			}

			await EntityAccessService.Remove(entity, cancellationToken);
			await EntityAccessService.SaveChanges(cancellationToken);

			return new Success();
		}
		catch (OperationCanceledException)
		{
			return new Canceled();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			LogConcurrencyException(ex, "delete", entityId);

			return new Conflict();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "delete", entityId);

			return new UnexpectedException();
		}
	}
}
