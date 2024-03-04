using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<ResourceId, Resource>, Resource>]
public sealed record class Resource : Entity<ResourceId>
{
	public string Name { get; set; }
	public string? Description { get; set; }
	public int Quantity { get; set; }
	public string Unit { get; set; }
}
