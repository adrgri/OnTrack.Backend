using OneOf;

namespace OnTrack.Backend.Api.OneOf;

public static class OneOfExtensions
{
	public static void AssignIfSucceeded<TItem, TError>(this OneOf<TItem, TError> itemToAssignOrError, Action<TItem> assigner)
	{
		itemToAssignOrError.Switch(
			item => assigner(item),
			_ => { });
	}

	public static async Task<TResult> MatchAsync<T0, T1, TResult>(this Task<OneOf<T0, T1>> task, Func<T0, Task<TResult>> f0, Func<T1, Task<TResult>> f1)
	{
		return await (await task).Match(f0, f1);
	}

	public static async Task<TResult> MatchAsync<T0, T1, T2, TResult>(this Task<OneOf<T0, T1, T2>> task, Func<T0, Task<TResult>> f0, Func<T1, Task<TResult>> f1, Func<T2, Task<TResult>> f2)
	{
		return await (await task).Match(f0, f1, f2);
	}
}
