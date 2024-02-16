using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.ComponentModel.DataAnnotations;

// I have not tested how this attribute will behave when applied to fields or parameters, so I removed those from the "validOn" attribute target types
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequiredWhenAttribute<T>(Predicate<T> shouldValidate)
	: RequiredAttribute
{
	protected Predicate<T> ShouldValidate { get; init; } = shouldValidate;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		T obj = (T)validationContext.ObjectInstance;

		return ShouldValidate(obj) ? base.IsValid(value, validationContext) : ValidationResult.Success;
	}
}
