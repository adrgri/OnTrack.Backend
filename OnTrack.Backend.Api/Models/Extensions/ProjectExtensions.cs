namespace OnTrack.Backend.Api.Models.Extensions;

public static class ProjectExtensions
{
	public static (int CompletedItemsCount, int TotalItemsCount) Metrics(this Project project)
	{
		(int CompletedItemsCount, int TotalItemsCount) output = (0, 0);

		if (project.Tasks?.Count is null or 0)
		{
			return output;
		}

		HashSet<TaskId> countedItems = [];

		return project.Tasks.Aggregate(output, ((int CompletedItemsCount, int TotalItemsCount) output, Task task) =>
		{
			(int completedItemsCount, int totalItemsCount) = task.Metrics(countedItems);

			output.CompletedItemsCount += completedItemsCount;
			output.TotalItemsCount += totalItemsCount;

			return output;
		});
	}

	public static Progress Progress(this Project project)
	{
		return new Progress(project.Metrics());
	}
}
