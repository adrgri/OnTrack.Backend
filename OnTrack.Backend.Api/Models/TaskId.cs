using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<TaskId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<TaskId>))]
public sealed record class TaskId : StronglyTypedId
{
	public static implicit operator TaskId(Guid value)
	{
		return new(value);
	}

	public static implicit operator Guid(TaskId id)
	{
		return id.Value;
	}
}
