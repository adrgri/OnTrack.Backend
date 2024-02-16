using System.ComponentModel;
using System.Globalization;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.ComponentModel;

public sealed class StronglyTypedIdTypeConverter<TStronglyTypedId> : TypeConverter
	where TStronglyTypedId : IStronglyTypedId, new()
{
	private static readonly Type[] _supportedConversionTypes =
	[
		typeof(string),
		typeof(Guid),
	];

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return _supportedConversionTypes.Contains(sourceType) || base.CanConvertFrom(context, sourceType);
	}

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		return value switch
		{
			string stringValue => new TStronglyTypedId()
			{
				Value = Guid.Parse(stringValue)
			},
			Guid guidValue => new TStronglyTypedId()
			{
				Value = guidValue
			},
			_ => base.ConvertFrom(context, culture, value)
		};
	}
}
