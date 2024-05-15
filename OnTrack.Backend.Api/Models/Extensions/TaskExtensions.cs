namespace OnTrack.Backend.Api.Models.Extensions;

public static class TaskExtensions
{
	public static (int CompletedItemsCount, int TotalItemsCount) Metrics(this Task task, HashSet<TaskId> countedItems)
	{
		// Analyze each task only once
		if (countedItems.Add(task.Id) is false)
		{
			return (0, 0);
		}

		(int CompletedItemsCount, int TotalItemsCount) output = (task.IsCompleted ? 1 : 0, 1);

		if (task.Subtasks?.Count is null or 0)
		{
			return output;
		}

		return task.Subtasks.Aggregate(output, ((int CompletedItemsCount, int TotalItemsCount) output, Task subtask) =>
		{
			(int completedItemsCount, int totalItemsCount) = subtask.Metrics(countedItems);

			output.CompletedItemsCount += completedItemsCount;
			output.TotalItemsCount += totalItemsCount;

			return output;
		});
	}

	public static (int CompletedItemsCount, int TotalItemsCount) Metrics(this Task task)
	{
		HashSet<TaskId> countedItems = [];

		return task.Metrics(countedItems);
	}

	public static Progress Progress(this Task task)
	{
		return new Progress(task.Metrics());
	}
}
