using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public record class ProjectDto : IDto
{
	public string Title { get; set; }
	public string? Description { get; set; }
	//public PathString? ImagePath { get; set; }
	public ICollection<IdentitySystemObjectId>? MemberIds { get; set; }
	public ICollection<TaskId>? TaskIds { get; set; }
}
