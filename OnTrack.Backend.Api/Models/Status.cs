using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<EntityStronglyTypedIdConfiguration<StatusId, Status>, Status>()]
public sealed class Status : IEntity<StatusId>
{
	public StatusId Id { get; init; } = new StatusId();
	public required string Name { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<StatusId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<StatusId>))]
public sealed record class StatusId : StronglyTypedId;

//public sealed class StatusConfiguration : EntityStronglyTypedIdConfiguration<StatusId, Status>;
