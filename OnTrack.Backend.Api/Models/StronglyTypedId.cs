using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

public abstract record class StronglyTypedId : IStronglyTypedId, IComparable<StronglyTypedId>, IEquatable<StronglyTypedId>
{
	public Guid Value { get; init; } = Guid.NewGuid();

	public int CompareTo(StronglyTypedId? other)
	{
		return Value.CompareTo(other?.Value);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}

public sealed class StronglyTypedIdJsonConverter<TStronglyTypedId> : JsonConverter<TStronglyTypedId>
	where TStronglyTypedId : IStronglyTypedId, new()
{
	public override TStronglyTypedId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new TStronglyTypedId()
		{
			Value = reader.GetGuid()
		};
	}

	public override void Write(Utf8JsonWriter writer, TStronglyTypedId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value);
	}
}

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
