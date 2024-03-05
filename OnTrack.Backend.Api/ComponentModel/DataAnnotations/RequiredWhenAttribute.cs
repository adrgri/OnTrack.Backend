using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.ComponentModel.DataAnnotations;

// TODO: I have not tested how this attribute will behave when applied to fields or parameters, so I removed those from the "validOn" attribute target types
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequiredWhenAttribute<TContainer>(Predicate<TContainer> shouldValidate, string explanationWhenValidationFailed)
	: ValidationAttribute(string.Format(_errorMessage, explanationWhenValidationFailed))
{
	private const string _errorMessage = "This property is required to have a value because {0}.";

	protected Predicate<TContainer> ShouldValidate { get; } = shouldValidate;

	public bool AllowEmptyStrings { get; init; }

	private RequiredAttribute _requiredAttribute => new() { AllowEmptyStrings = AllowEmptyStrings };

	public override bool IsValid(object? value)
	{
		return _requiredAttribute.IsValid(value);
	}

	protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
	{
		TContainer validatedObject = (TContainer)validationContext!.ObjectInstance;

		return ShouldValidate(validatedObject) ? base.IsValid(value, validationContext) : ValidationResult.Success;
	}
}
