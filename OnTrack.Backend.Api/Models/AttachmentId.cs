using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<AttachmentId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<AttachmentId>))]
public sealed record class AttachmentId : StronglyTypedId
{
	public static implicit operator AttachmentId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(AttachmentId id)
	{
		return id.Value;
	}
}
