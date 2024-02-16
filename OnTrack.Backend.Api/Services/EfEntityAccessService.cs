using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

using Task = System.Threading.Tasks.Task;

namespace OnTrack.Backend.Api.Services;

// TODO: Add remarks telling that this class relies on the DbContext to have appropriate DbSet<TEntity> properties, maybe add a sanity check in the app startup to check for those properties
public class EfEntityAccessService<TDbContext, TEntity, TEntityId>(TDbContext context)
	: IEntityAccessService<TEntity, TEntityId>
	where TDbContext : DbContext
	where TEntity : class, IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
{
	private readonly TDbContext _context = context;

	public virtual async Task Add(TEntity entity)
	{
		_ = await _context.AddAsync(entity);
	}

	public virtual async Task<TEntity?> Find(TEntityId id)
	{
		return await _context.FindAsync<TEntity>(id);
	}

	public async Task<bool> Exists(TEntityId id)
	{
		return (await _context.FindAsync<TEntity>(id)) is not null;

		// TODO: Zrób benchmark i sprawdź, która metoda będzie szybsza, czy ta powyżej czy ta poniżej. Może porównaj też ze starszymy wersjami EF?
		//return await _context.Set<TEntity>().AnyAsync(e => e.Id.Equals(id));
	}

	public virtual async Task<IEnumerable<TEntity>> GetAll()
	{
		return await _context.Set<TEntity>().ToListAsync();
	}

	public virtual Task Update(TEntity entity)
	{
		_context.Entry(entity).State = EntityState.Modified;

		return Task.CompletedTask;
	}

	public virtual Task Remove(TEntity entity)
	{
		_ = _context.Remove(entity);

		return Task.CompletedTask;
	}

	public virtual async Task SaveChanges()
	{
		_ = await _context.SaveChangesAsync();
	}
}
