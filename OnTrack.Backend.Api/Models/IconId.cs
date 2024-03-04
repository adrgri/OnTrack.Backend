using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<IconId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<IconId>))]
public sealed record class IconId : StronglyTypedId
{
	public static implicit operator IconId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(IconId id)
	{
		return id.Value;
	}
}
