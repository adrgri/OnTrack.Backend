using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ProjectId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ProjectId>))]
public sealed record class ProjectId : StronglyTypedId
{
	public static implicit operator ProjectId(Guid value)
	{
		return new()
		{
			Value = value
		};
	}

	public static implicit operator Guid(ProjectId id)
	{
		return id.Value;
	}
}
