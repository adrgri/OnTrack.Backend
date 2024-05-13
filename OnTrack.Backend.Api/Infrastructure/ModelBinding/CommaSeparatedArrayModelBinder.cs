using System.ComponentModel;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnTrack.Backend.Api.Infrastructure.ModelBinding;

public sealed class CommaSeparatedArrayModelBinder : IModelBinder
{
	private const char _arrayElementsSeparator = ',';

	public SysTask BindModelAsync(ModelBindingContext bindingContext)
	{
		object?[] convertedValues = [];

		ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

		Type elementType = bindingContext.ModelType.GetElementType() ?? throw new UnreachableException();

		TypeConverter converter = TypeDescriptor.GetConverter(elementType);

		string? stringValue = valueProviderResult.FirstValue;

		if (string.IsNullOrWhiteSpace(stringValue) == false)
		{
			try
			{
				convertedValues = Array.ConvertAll(
					stringValue.Split(_arrayElementsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
					converter.ConvertFromString);
			}
			catch (FormatException)
			{
				_ = bindingContext.ModelState.TryAddModelError(
					bindingContext.ModelName,
					"Data provided for this parameter is not in the correct format, the correct format is: \"Value1,Value2,ValueN\".");
			}
		}

		if (bindingContext.ModelState.IsValid)
		{
			Array convertedValuesWithCorrectType = Array.CreateInstance(elementType, convertedValues.Length);

			convertedValues.ToArray().CopyTo(convertedValuesWithCorrectType, 0);

			bindingContext.Result = ModelBindingResult.Success(convertedValuesWithCorrectType);
		}

		return SysTask.CompletedTask;
	}
}
