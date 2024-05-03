namespace OnTrack.Backend.Api.Models.Extensions;

public static class ProjectExtensions
{
	public static int NumberOfTasks(this Project project)
	{
		int numberOfTasks = 0;

		if (project.Tasks is null)
		{
			return numberOfTasks;
		}

		foreach (Task task in project.Tasks)
		{
			numberOfTasks += task.NumberOfSubtasks();
		}

		return numberOfTasks;
	}
}
