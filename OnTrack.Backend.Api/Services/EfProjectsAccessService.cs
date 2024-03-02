using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfProjectsAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<ProjectId, Project, TDbContext>(context)
	where TDbContext : DbContext
{
	private void SetNestedAppUsersState(ICollection<AppUser> existingAppUsers, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (AppUser member in existingAppUsers)
		{
			Context.Entry(member).State = EntityState.Modified;
		}
	}

	private void SetNestedMilestonesState(ICollection<Milestone> existingMilestones, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (Milestone milestone in existingMilestones)
		{
			Context.Entry(milestone).State = EntityState.Modified;
		}
	}

	private void SetNestedEntitesState(Project entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedAppUsersState(entity.Members, cancellationToken);

		if (entity.Milestones is not null)
		{
			SetNestedMilestonesState(entity.Milestones, cancellationToken);
		}
	}

	public override async SysTask Add(Project entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		await base.Add(entity, cancellationToken);
	}

	public override async Task<Project?> Find(ProjectId id, CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(project => project.Members)
			.Include(project => project.Milestones)
			.Where(project => project.Id == id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public override async Task<IEnumerable<Project>> GetAll(CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(project => project.Members)
			.Include(project => project.Milestones)
			.ToListAsync(cancellationToken);
	}

	public override async SysTask Remove(Project entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		await base.Remove(entity, cancellationToken);
	}
}
