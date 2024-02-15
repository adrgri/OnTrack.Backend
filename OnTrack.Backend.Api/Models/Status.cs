using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<StatusId, Status>, Status>()]
public sealed record class Status : IEntity<StatusId>
{
	public StatusId Id { get; init; } = new StatusId();
	public string Name { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<StatusId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<StatusId>))]
public sealed record class StatusId : StronglyTypedId;
