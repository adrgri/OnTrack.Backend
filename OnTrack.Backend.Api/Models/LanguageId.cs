using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<LanguageId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<LanguageId>))]
public sealed record class LanguageId : StronglyTypedId
{
	public static implicit operator LanguageId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(LanguageId id)
	{
		return id.Value;
	}
}
