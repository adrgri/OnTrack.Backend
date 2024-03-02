using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

// TODO: Add remarks telling that this class relies on the DbContext to have appropriate DbSet<TEntity> properties, maybe add a sanity check in the app startup to check for those properties
/*/ This class can not be abstract since DI container must be able to create instances of it (if you don't create concrete child classes for every Entity) /*/
public class EfEntityAccessService<TEntityId, TEntity, TDbContext>(TDbContext context)
	: IEntityAccessService<TEntityId, TEntity>
	where TEntityId : IStronglyTypedId
	where TEntity : class, IEntity<TEntityId>
	where TDbContext : DbContext
{
	protected TDbContext Context { get; } = context;

	public virtual async SysTask Add(TEntity entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

#if ApplicationGeneratedGuidIdsAllowed == false
		Guard.Against.InsertingEntitiesWithApplicationGeneratedGuidIds<TEntity, TEntityId>(entity);
#endif

		_ = await Context.AddAsync(entity, cancellationToken);
	}

	public virtual async Task<TEntity?> Find(TEntityId id, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return await Context.FindAsync<TEntity>([id], cancellationToken);
	}

	public virtual async Task<bool> Exists(TEntityId id, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return (await Find(id, cancellationToken)) is not null;

		// TODO: Zrób benchmark i sprawdź, która metoda będzie szybsza, czy ta powyżej czy ta poniżej. Może porównaj też ze starszymy wersjami EF?
		//return await _context.Set<TEntity>().AnyAsync(e => e.Id.Equals(id));
	}

	public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return await Context.Set<TEntity>().ToListAsync(cancellationToken);
	}

	public virtual SysTask Update(TEntity entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		Context.Entry(entity).State = EntityState.Modified;

		return SysTask.CompletedTask;
	}

	public virtual SysTask Remove(TEntity entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_ = Context.Remove(entity);

		return SysTask.CompletedTask;
	}

	public virtual IQueryable<TEntity> Query(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return Context.Set<TEntity>()
			.AsQueryable();
	}

	public virtual async SysTask SaveChanges(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_ = await Context.SaveChangesAsync(cancellationToken);
	}
}
