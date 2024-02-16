using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class TaskDto : IDto
{
	public MilestoneId MilestoneId { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? DueDate { get; set; }
	public IconId? IconId { get; set; }
	public bool IsCompleted { get; set; }
	public ICollection<ResourceId>? AssignedResourceIds { get; set; }
	public ICollection<AttachmentId>? AttachmentIds { get; set; }
	public ICollection<TaskId>? SubtaskIds { get; set; }
}
