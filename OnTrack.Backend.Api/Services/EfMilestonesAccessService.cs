using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfMilestonesAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<MilestoneId, Milestone, TDbContext>(context)
	where TDbContext : DbContext
{
	private void SetNestedProjectState(Project existingProject, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		Context.Entry(existingProject).State = EntityState.Modified;
	}

	private void SetNestedTasksState(ICollection<Task> existingTasks, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (Task existingTask in existingTasks)
		{
			Context.Entry(existingTask).State = EntityState.Modified;
		}
	}

	private void SetNestedEntitesState(Milestone entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedProjectState(entity.Project, cancellationToken);

		if (entity.Tasks is not null)
		{
			SetNestedTasksState(entity.Tasks, cancellationToken);
		}
	}

	public override async SysTask Add(Milestone entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		await base.Add(entity, cancellationToken);
	}

	public override async Task<Milestone?> Find(MilestoneId id, CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(milestone => milestone.Project)
			.Include(milestone => milestone.Status)
			.Include(milestone => milestone.Tasks)
			.Where(milestone => milestone.Id == id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public override async Task<IEnumerable<Milestone>> GetAll(CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(milestone => milestone.Project)
			.Include(milestone => milestone.Status)
			.Include(milestone => milestone.Tasks)
			.ToListAsync(cancellationToken);
	}

	public override async SysTask Remove(Milestone entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		await base.Remove(entity, cancellationToken);
	}
}
