using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class MustContainAtLeastOneElementAttribute()
	: ValidationAttribute(_errorMessage)
{
	private const string _errorMessage = "This collection is required to contain at least one element.";

	public override bool IsValid(object? value)
	{
		if (value is ICollection collection)
		{
			return collection.Count is not 0;
		}

		return value is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
	}
}
