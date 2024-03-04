using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ResourceId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ResourceId>))]
public sealed record class ResourceId : StronglyTypedId
{
	public static implicit operator ResourceId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(ResourceId id)
	{
		return id.Value;
	}
}
