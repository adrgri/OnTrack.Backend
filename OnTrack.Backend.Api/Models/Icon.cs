using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<IconConfiguration, Icon>]
public sealed record class Icon : IEntity<IconId>
{
	public IconId Id { get; set; }
	public string Name { get; set; }
	public PathString FilePath { get; set; }
}
