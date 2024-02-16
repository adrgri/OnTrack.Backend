using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class MilestoneDto : IDto
{
	public ProjectId ProjectId { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public StatusId? StatusId { get; set; }
	public ICollection<TaskId>? TaskIds { get; set; }
}
