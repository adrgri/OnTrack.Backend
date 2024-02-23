using OneOf;
using OneOf.Types;

namespace OnTrack.Backend.Api.OneOf;

public static class OneOfExtensions
{
	public static void AssignIfSucceeded<T>(this OneOf<T, Error> validationResult, Action<T> assigning)
	{
		validationResult.Switch(
			existingMilestone => assigning(existingMilestone),
			_ => { });
	}
}
