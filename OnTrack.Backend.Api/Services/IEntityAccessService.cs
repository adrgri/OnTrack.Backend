using OnTrack.Backend.Api.Models;

using Task = System.Threading.Tasks.Task;

namespace OnTrack.Backend.Api.Services;

public interface IEntityAccessService<TEntity, in TEntityId>
	where TEntity : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
{
	// TODO: Add methods to get the outcome of the operation, success or failure with description. This can be a discriminated union
	Task Add(TEntity entity);
	Task<TEntity?> Find(TEntityId id);
	Task<bool> Exists(TEntityId id);
	Task<IEnumerable<TEntity>> GetAll();
	Task Update(TEntity entity);
	Task Remove(TEntity entity);

	Task SaveChanges();
}
