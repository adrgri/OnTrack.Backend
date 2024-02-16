using OnTrack.Backend.Api.ComponentModel.DataAnnotations;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class ProjectDto : IDto
{
	public string Title { get; set; }
	public string? Description { get; set; }
	public PathString? ImagePath { get; set; }
	[MustContainAtLeastOneElement]
	public ICollection<IdentitySystemObjectId> MemberIds { get; set; }
	public ICollection<MilestoneId>? MilestoneIds { get; set; }
}
