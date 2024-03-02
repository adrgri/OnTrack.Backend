using System.Runtime.CompilerServices;

using OneOf;

using OnTrack.Backend.Api.Models;
using OnTrack.Backend.Api.Services;

namespace OnTrack.Backend.Api.Validation;

public class EntityByIdExistenceValidator<TEntityId, TEntity>(IEntityAccessService<TEntityId, TEntity> entityAccessService)
	: IAsyncCollectionValidator<TEntityId, OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>
{
	protected const string entityIdDoesNotExistErrorMessageTemplate = "{0} with this Id does not exist.";

	protected IEntityAccessService<TEntityId, TEntity> EntityAccessService { get; } = entityAccessService;

	public async Task<OneOf<TEntity, EntityIdErrorsDescription<TEntityId>>> Validate(TEntityId item, CancellationToken cancellationToken)
	{
		TEntity? entity = await EntityAccessService.Find(item, cancellationToken);

		cancellationToken.ThrowIfCancellationRequested();

		return entity switch
		{
			not null => entity,
			null => new EntityIdErrorsDescription<TEntityId>(item, [string.Format(entityIdDoesNotExistErrorMessageTemplate, typeof(TEntity).Name)])
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
