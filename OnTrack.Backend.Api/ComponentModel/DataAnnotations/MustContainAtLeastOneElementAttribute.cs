using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class MustContainAtLeastOneElementAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		if (value is ICollection collection)
		{
			return collection.Count is not 0;
		}

		return value is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
	}
}
