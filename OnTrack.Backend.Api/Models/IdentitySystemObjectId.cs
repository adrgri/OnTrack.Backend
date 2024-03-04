using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<IdentitySystemObjectId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<IdentitySystemObjectId>))]
public sealed record class IdentitySystemObjectId : StronglyTypedId
{
	public static implicit operator IdentitySystemObjectId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(IdentitySystemObjectId id)
	{
		return id.Value;
	}
}
