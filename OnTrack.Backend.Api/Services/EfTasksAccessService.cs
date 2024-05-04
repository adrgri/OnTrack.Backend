using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfTasksAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TaskId, Task, TDbContext>(context)
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

	private void SetNestedEntitesState(Task entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedProjectState(entity.Project, cancellationToken);

		if (entity.Subtasks is not null)
		{
			SetNestedTasksState(entity.Subtasks, cancellationToken);
		}
	}

	public override async SysTask Add(Task entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		await base.Add(entity, cancellationToken);
	}

	public override async Task<Task?> Find(TaskId id, CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(task => task.Project)
			.Include(task => task.Status)
			.Include(task => task.Icon)
			.Include(task => task.AssignedMembers)
			.Include(task => task.AssignedResources)
			.Include(task => task.Attachments)
			.Include(task => task.Subtasks)
			.Where(task => task.Id == id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public override async Task<IEnumerable<Task>> GetAll(CancellationToken cancellationToken)
	{
		return await Query(cancellationToken)
			.Include(task => task.Project)
			.Include(task => task.Status)
			.Include(task => task.Icon)
			.Include(task => task.AssignedMembers)
			.Include(task => task.AssignedResources)
			.Include(task => task.Attachments)
			.Include(task => task.Subtasks)
			.ToListAsync(cancellationToken);
	}

	//// TODO: Delete the task and all its subtasks, this may not be needed since EF Core might be smart enough to do this for us
	//private async SysTask RmoveTaskTree(Task entity)
	//{

	//}

	public override async SysTask Remove(Task entity, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		SetNestedEntitesState(entity, cancellationToken);

		//RmoveTaskTree(entity);

		await base.Remove(entity, cancellationToken);
	}
}
