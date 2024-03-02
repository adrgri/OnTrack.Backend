using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public interface IEntityAccessService<in TEntityId, TEntity>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>
{
	// TODO: Add methods to get the outcome of the operation, success or failure with description. This can be a discriminated union
	SysTask Add(TEntity entity, CancellationToken cancellationToken);
	SysTask Add(TEntity entity)
	{
		return Add(entity, CancellationToken.None);
	}

	Task<TEntity?> Find(TEntityId id, CancellationToken cancellationToken);
	Task<TEntity?> Find(TEntityId id)
	{
		return Find(id, CancellationToken.None);
	}

	Task<bool> Exists(TEntityId id, CancellationToken cancellationToken);
	Task<bool> Exists(TEntityId id)
	{
		return Exists(id, CancellationToken.None);
	}

	Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken);
	Task<IEnumerable<TEntity>> GetAll()
	{
		return GetAll(CancellationToken.None);
	}

	SysTask Update(TEntity entity, CancellationToken cancellationToken);
	SysTask Update(TEntity entity)
	{
		return Update(entity, CancellationToken.None);
	}

	SysTask Remove(TEntity entity, CancellationToken cancellationToken);
	SysTask Remove(TEntity entity)
	{
		return Remove(entity, CancellationToken.None);
	}

	IQueryable<TEntity> Query(CancellationToken cancellationToken);
	IQueryable<TEntity> Query()
	{
		return Query(CancellationToken.None);
	}

	SysTask SaveChanges(CancellationToken cancellationToken);
	SysTask SaveChanges()
	{
		return SaveChanges(CancellationToken.None);
	}
}
