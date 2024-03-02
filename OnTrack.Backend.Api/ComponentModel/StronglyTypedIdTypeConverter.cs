using System.ComponentModel;
using System.Globalization;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.ComponentModel;

public sealed class StronglyTypedIdTypeConverter<TStronglyTypedId> : TypeConverter
	where TStronglyTypedId : IStronglyTypedId, new()
{
#pragma warning disable S2743 // Static fields should not be used in generic types -> I know that this field is not shared among generic instances of this class, it's fine in this case
	private static readonly Type[] _supportedConversionTypes =
	[
		typeof(string),
		typeof(Guid),
	];
#pragma warning restore S2743 // Static fields should not be used in generic types

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
