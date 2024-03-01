using System.Text.Json;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Text.Json.Serialization;

public sealed class StronglyTypedIdJsonConverter<TStronglyTypedId> : JsonConverter<TStronglyTypedId>
	where TStronglyTypedId : IStronglyTypedId, new()
{
	// TODO: A co jeśli reader.GetGuid() rzuci format exception?
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
