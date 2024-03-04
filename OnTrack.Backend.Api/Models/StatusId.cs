using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<StatusId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<StatusId>))]
public sealed record class StatusId : StronglyTypedId
{
	public static implicit operator StatusId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(StatusId id)
	{
		return id.Value;
	}
}
