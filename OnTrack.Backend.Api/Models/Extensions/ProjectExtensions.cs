namespace OnTrack.Backend.Api.Models.Extensions;

public static class ProjectExtensions
{
	public static int NumberOfMilestones(this Project project)
	{
		return project.Milestones?.Count ?? 0;
	}

	public static int NumberOfTasks(this Project project)
	{
		int numberOfTasks = 0;

		if (project.Milestones is null)
		{
			return numberOfTasks;
		}

		foreach (Milestone milestone in project.Milestones)
		{
			numberOfTasks += milestone.NumberOfTasks();
		}

		return numberOfTasks;
	}
}
