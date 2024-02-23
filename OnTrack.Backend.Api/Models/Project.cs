using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.ComponentModel.DataAnnotations;
using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<ProjectConfiguration, Project>]
public sealed record class Project : IEntity<ProjectId>
{
	public ProjectId Id { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	//public PathString? ImagePath { get; set; }
	[MustContainAtLeastOneElement]
	public ICollection<AppUser> Members { get; set; }
	public ICollection<Milestone>? Milestones { get; set; }
}
