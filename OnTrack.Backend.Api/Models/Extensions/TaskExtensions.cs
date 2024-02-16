namespace OnTrack.Backend.Api.Models.Extensions;

public static class TaskExtensions
{
	public static int NumberOfTasks(this Task task)
	{
		int numberOfTasks = 1;

		if (task.Subtasks is null)
		{
			return numberOfTasks;
		}

		foreach (Task subtask in task.Subtasks)
		{
			numberOfTasks += subtask.NumberOfTasks();
		}

		return numberOfTasks;
	}
}
