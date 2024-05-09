using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OneOf;
using OneOf.Types;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.DataAccess;
using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.OneOf;
using OnTrack.Backend.Api.Services;
using OnTrack.Backend.Api.Threading;
using OnTrack.Backend.Api.Validation;

namespace OnTrack.Backend.Api.Controllers;

// TODO: Add a constructor parameter to give this class an ability to validate the entities by taking an async validator
// + add virtual methods to 1. prepare them for validation 2. handle and post validation results, maybe adding to the ModelState can be done in the base class
// as virtual implementation and the derived class can override it to add more specific error handling should they need it
public abstract class GenericController<TEntityId, TEntity, TDto, TController>(
	ILogger<TController> logger,
	IEntityAccessService<TEntityId, TEntity> entityAccessService,
	IMapper<TEntityId, TEntity, TDto> mapper,
	IAsyncCollectionValidator<TEntityId, OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>> entityCollectionValidator)
	: ControllerBase
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>
	where TDto : IDto
	where TController : ControllerBase
{
	private const string _operationCanceledExceptionMessageLoggerTemplate = "Canceled {ActionName} action on the entity {EntityName}";
	private const string _concurrencyErrorMessageLoggerTemplate = "Concurrency exception occurred while trying to {ActionName} the {EntityName}";
	private const string _unexpectedExceptionMessageLoggerTemplate = "Unexpected exception occurred while trying to {ActionName} the {EntityName}";

	protected const string operationCanceledExceptionMessageLoggerTemplate = $"{_operationCanceledExceptionMessageLoggerTemplate}.";
	protected const string concurrencyErrorMessageLoggerTemplate = $"{_concurrencyErrorMessageLoggerTemplate}.";
	protected const string unexpectedExceptionMessageLoggerTemplate = $"{_unexpectedExceptionMessageLoggerTemplate}.";

	protected const string operationCanceledExceptionMessageWithEntityIdLoggerTemplate = _operationCanceledExceptionMessageLoggerTemplate + " with id {EntityId}.";
	protected const string concurrencyErrorMessageWithEntityIdLoggerTemplate = _concurrencyErrorMessageLoggerTemplate + " with id {EntityId}.";
	protected const string unexpectedExceptionMessageWithEntityIdLoggerTemplate = _unexpectedExceptionMessageLoggerTemplate + " with id {EntityId}.";

	protected static readonly string entityName = typeof(TEntity).Name;

	protected ILogger<TController> Logger { get; } = logger;
	protected IEntityAccessService<TEntityId, TEntity> EntityAccessService { get; } = entityAccessService;
	protected IMapper<TEntityId, TEntity, TDto> Mapper { get; } = mapper;
	protected IAsyncCollectionValidator<TEntityId, OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>> EntityCollectionValidator { get; } = entityCollectionValidator;

	protected virtual void LogOperationCanceledException(OperationCanceledException ex, string actionName)
	{
		Logger.LogError(ex, operationCanceledExceptionMessageLoggerTemplate, actionName, entityName);
	}

	protected virtual void LogOperationCanceledException(OperationCanceledException ex, string actionName, TEntityId entityId)
	{
		Logger.LogError(ex, operationCanceledExceptionMessageWithEntityIdLoggerTemplate, actionName, entityName, entityId);
	}

	protected virtual void LogConcurrencyException(DbUpdateConcurrencyException ex, string actionName)
	{
		Logger.LogError(ex, concurrencyErrorMessageLoggerTemplate, actionName, entityName);
	}

	protected virtual void LogConcurrencyException(DbUpdateConcurrencyException ex, string actionName, TEntityId entityId)
	{
		Logger.LogError(ex, concurrencyErrorMessageWithEntityIdLoggerTemplate, actionName, entityName, entityId);
	}

	protected virtual void LogUnexpectedException(Exception ex, string actionName)
	{
		Logger.LogError(ex, unexpectedExceptionMessageLoggerTemplate, actionName, entityName);
	}

	protected virtual void LogUnexpectedException(Exception ex, string actionName, TEntityId entityId)
	{
		Logger.LogError(ex, unexpectedExceptionMessageWithEntityIdLoggerTemplate, actionName, entityName, entityId);
	}

	//protected async Task<OneOf<TEntity, ValidationFailure, Conflict, Canceled, UnexpectedException>> UnsafeWrapper(Func<Task<OneOf<TEntity, ValidationFailure, Conflict, Canceled, UnexpectedException>>> unsafeMethod, string actionName)
	//{
	//	try
	//	{
	//		return await unsafeMethod();
	//	}
	//	catch (OperationCanceledException)
	//	{
	//		return new Canceled();
	//	}
	//	catch (DbUpdateConcurrencyException ex)
	//	{
	//		LogConcurrencyException(ex, actionName);

	//		return new Conflict();
	//	}
	//	catch (Exception ex)
	//	{
	//		LogUnexpectedException(ex, actionName);

	//		return new UnexpectedException();
	//	}
	//}

	//protected async Task<OneOf<TEntity, NotFound, Conflict, Canceled, UnexpectedException>> UnsafeWrapper(Func<Task<OneOf<TEntity, NotFound, Conflict, Canceled, UnexpectedException>>> unsafeMethod, string actionName, TEntityId entityId)
	//{
	//	try
	//	{
	//		return await unsafeMethod();
	//	}
	//	catch (OperationCanceledException)
	//	{
	//		return new Canceled();
	//	}
	//	catch (DbUpdateConcurrencyException ex)
	//	{
	//		LogConcurrencyException(ex, actionName, entityId);

	//		return new Conflict();
	//	}
	//	catch (Exception ex)
	//	{
	//		LogUnexpectedException(ex, actionName, entityId);

	//		return new UnexpectedException();
	//	}
	//}

	protected virtual ValidationFailure RegisterEntityIdErrors<TArbitraryEntityId>(EntityIdErrorsDescription<TArbitraryEntityId> entityIdErrorsDescription)
		where TArbitraryEntityId : IStronglyTypedId
	{
		foreach (string errorDescription in entityIdErrorsDescription.ErrorsDescription)
		{
			string entityIdAsString = entityIdErrorsDescription.EntityId.ToString();

			ModelState.AddModelError(entityIdAsString, errorDescription);
		}

		return new ValidationFailure();
	}

	protected async Task<OneOf<TArbitraryEntity, NotFound>> ValidateEntityExistence<TArbitraryEntity, TArbitraryEntityId>(
		TArbitraryEntityId entityId,
		IAsyncValidator<TArbitraryEntityId, OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>>> entityExistenceValidator)
		where TArbitraryEntity : IEntity<TArbitraryEntityId>
		where TArbitraryEntityId : IStronglyTypedId
	{
		OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>> validationResult = await entityExistenceValidator.Validate(entityId);

		return validationResult.Match<OneOf<TArbitraryEntity, NotFound>>(
			existingEntity => existingEntity,
			entityIdErrorsDescription =>
			{
				_ = RegisterEntityIdErrors(entityIdErrorsDescription);

				return new NotFound();
			});
	}

	protected async SysTask ValidateEntitiesExistence<TArbitraryEntity, TArbitraryEntityId>(
		IEnumerable<TArbitraryEntityId> entityIdsToValidate,
		ICollection<TArbitraryEntity> existingEntities,
		IAsyncCollectionValidator<TArbitraryEntityId, OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>>> entitiesExistenceValidator)
		where TArbitraryEntity : IEntity<TArbitraryEntityId>
		where TArbitraryEntityId : IStronglyTypedId
	{
		await foreach (OneOf<TArbitraryEntity, EntityIdErrorsDescription<TArbitraryEntityId>> validationResult in entitiesExistenceValidator.Validate(entityIdsToValidate))
		{
			validationResult.Switch(
				existingEntities.Add,
				entityIdErrorsDescription => RegisterEntityIdErrors(entityIdErrorsDescription));
		}
	}

	// TODO: Replace validators with converters that will convert a dto to the entity and entity to a dtoWithId
	// In the current state validators have side effects since the entity will mutate as it is being validated, this is against functional design and will cause bugs down the road
	protected abstract Task<OneOf<TEntity, ValidationFailure>> ConvertToNewDomainModel(TDto entityDto);
	protected abstract Task<OneOf<TEntity, NotFound, ValidationFailure>> ConvertToNewDomainModel(TEntityId entityId, TDto entityDto);

	protected async Task<OneOf<TEntity, ValidationFailure, Conflict, Canceled, UnexpectedException>> Post(TDto entityDto, CancellationToken cancellationToken)
	{
		try
		{
			return await ConvertToNewDomainModel(entityDto).MatchAsync<TEntity, ValidationFailure, OneOf<TEntity, ValidationFailure, Conflict, Canceled, UnexpectedException>>(async entity =>
			{
				await EntityAccessService.Add(entity, cancellationToken);
				await EntityAccessService.SaveChanges(cancellationToken);

				return entity;
			},
			(ValidationFailure validationFailure) => SysTask.FromResult<OneOf<TEntity, ValidationFailure, Conflict, Canceled, UnexpectedException>>(validationFailure));
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "insert");

			return new Canceled();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			LogConcurrencyException(ex, "insert");

			return new Conflict();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "insert");

			return new UnexpectedException();
		}
	}

	protected async Task<OneOf<TEntity, NotFound, Conflict, Canceled, UnexpectedException>> Get(TEntityId entityId, CancellationToken cancellationToken)
	{
		try
		{
			TEntity? entity = await EntityAccessService.Find(entityId, cancellationToken);

			return entity switch
			{
				null => new NotFound(),
				_ => entity
			};
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "get", entityId);

			return new Canceled();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			LogConcurrencyException(ex, "get", entityId);

			return new Conflict();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "get", entityId);

			return new UnexpectedException();
		}
	}

	protected async Task<OneOf<List<TEntity>, ValidationFailure, Conflict, Canceled, UnexpectedException>> GetMany(IEnumerable<TEntityId> entityIds, CancellationToken cancellationToken)
	{
		try
		{
			List<TEntity> entities = [];

			await ValidateEntitiesExistence(entityIds, entities, EntityCollectionValidator);

			cancellationToken.ThrowIfCancellationRequested();

			return ModelState.IsValid ? entities : new ValidationFailure();
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "get many");

			return new Canceled();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			LogConcurrencyException(ex, "get many");

			return new Conflict();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "get many");

			return new UnexpectedException();
		}
	}

	protected async Task<OneOf<List<TEntity>, Canceled, UnexpectedException>> GetAll(CancellationToken cancellationToken)
	{
		try
		{
			IEnumerable<TEntity> entities = await EntityAccessService.GetAll(cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();

			return entities.ToList();
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "get all");

			return new Canceled();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "get all");

			return new UnexpectedException();
		}
	}

	protected async Task<OneOf<TEntity, NotFound, ValidationFailure, Conflict, Canceled, UnexpectedException>> Put(TEntityId entityId, TDto entityDto, CancellationToken cancellationToken)
	{
		try
		{
			return await ConvertToNewDomainModel(entityId, entityDto).MatchAsync<TEntity, NotFound, ValidationFailure, OneOf<TEntity, NotFound, ValidationFailure, Conflict, Canceled, UnexpectedException>>(async entity =>
			{
				await EntityAccessService.Update(entity, cancellationToken);
				await EntityAccessService.SaveChanges(cancellationToken);

				return entity;
			},
			(NotFound notFound) => SysTask.FromResult<OneOf<TEntity, NotFound, ValidationFailure, Conflict, Canceled, UnexpectedException>>(notFound),
			(ValidationFailure validationFailure) => SysTask.FromResult<OneOf<TEntity, NotFound, ValidationFailure, Conflict, Canceled, UnexpectedException>>(validationFailure));
		}
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "update", entityId);

			return new Canceled();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			LogConcurrencyException(ex, "update", entityId);

			return new Conflict();
		}
		catch (Exception ex)
		{
			LogUnexpectedException(ex, "update", entityId);

			return new UnexpectedException();
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
		catch (OperationCanceledException ex)
		{
			LogOperationCanceledException(ex, "delete", entityId);

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
