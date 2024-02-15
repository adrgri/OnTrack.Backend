using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<ResourceId, Resource>, Resource>()]
public sealed record class Resource : IEntity<ResourceId>
{
	public ResourceId Id { get; init; }
	public string Name { get; set; }
	public string? Description { get; set; }
	public int Quantity { get; set; }
	public string Unit { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ResourceId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ResourceId>))]
public sealed record class ResourceId : StronglyTypedId;

