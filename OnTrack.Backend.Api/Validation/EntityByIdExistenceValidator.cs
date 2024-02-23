using System.Runtime.CompilerServices;

using OneOf;

using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Validation;

public class EntityByIdExistenceValidator<TEntityId, TEntity>(IEntityAccessService<TEntity, TEntityId> entityAccessService)
	: IAsyncCollectionValidator<TEntityId, OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>
{
	private const string _errorMessageTemplate = "{0} with this Id does not exist.";

	private readonly IEntityAccessService<TEntity, TEntityId> _entityAccessService = entityAccessService;

	public async Task<OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>> Validate(TEntityId item, CancellationToken cancellationToken)
	{
		TEntity? entity = await _entityAccessService.Find(item, cancellationToken);

		cancellationToken.ThrowIfCancellationRequested();

		return entity switch
		{
			not null => entity,
			null => new EntityIdErrorsDescription<TEntityId>(item, [string.Format(_errorMessageTemplate, typeof(TEntity).Name)])
		};
	}

	public async IAsyncEnumerable<OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>> Validate(IEnumerable<TEntityId> entityIds, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		foreach (TEntityId entityId in entityIds)
		{
			yield return await Validate(entityId, cancellationToken);
		}
	}
}
