namespace OnTrack.Backend.Api.Models.Extensions;

public static class MilestoneExtensions
{
	public static int NumberOfTasks(this Milestone milestone)
	{
		int numberOfTasks = 0;

		if (milestone.Tasks is null)
		{
			return numberOfTasks;
		}

		foreach (Task task in milestone.Tasks)
		{
			numberOfTasks += task.NumberOfTasks();
		}

		return numberOfTasks;
	}
}
