using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<IconConfiguration, Icon>]
public sealed record class Icon : Entity<IconId>
{
	public string Name { get; set; }
	//public PathString FilePath { get; set; }
}
