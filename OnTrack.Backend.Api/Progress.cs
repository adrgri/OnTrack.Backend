namespace OnTrack.Backend.Api;

public readonly record struct Progress(int CompletedItemsCount, int TotalItemsCount, Percentage CompletedItemsPercent)
{
	private static int ValidateCompletedItemsCountThrowOnError(int completedItemsCount, int totalItemsCount)
	{
		if (completedItemsCount > totalItemsCount)
		{
			throw new ArgumentException("Number of completed items can not be grater than the total number of items.", nameof(completedItemsCount));
		}

		return completedItemsCount;
	}

	private static Percentage CalculateCompletedItemsPercent(int completedItemsCount, int totalItemsCount)
	{
		return totalItemsCount is 0 ? 0 : (double)completedItemsCount / totalItemsCount;
	}

	public Progress(int completedItemsCount, int totalItemsCount)
		: this(ValidateCompletedItemsCountThrowOnError(completedItemsCount, totalItemsCount),
			  totalItemsCount,
		 CalculateCompletedItemsPercent(completedItemsCount, totalItemsCount))
	{
	}

	public Progress((int CompletedItemsCount, int TotalItemsCount) tuple)
		: this(tuple.CompletedItemsCount, tuple.TotalItemsCount)
	{
	}

	public static explicit operator Progress((int CompletedItemsCount, int TotalItemsCount) tuple)
	{
		return new(tuple);
	}

	public static explicit operator Percentage(Progress progress)
	{
		return progress.CompletedItemsPercent;
	}
}
