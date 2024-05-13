using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

// TODO: Zmień nazwę Taska na Todo?
public record class TaskDto : IDto
{
	public ProjectId ProjectId { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? DueDate { get; set; }
	public StatusId? StatusId { get; set; }
	public IconId? IconId { get; set; }
	public bool IsCompleted { get; set; }
	public ICollection<TaskId>? PredecessorIds { get; set; }
	public ICollection<TaskId>? SuccessorIds { get; set; }
	public ICollection<IdentitySystemObjectId>? AssignedMemberIds { get; set; }
	public ICollection<ResourceId>? AssignedResourceIds { get; set; }
	public ICollection<AttachmentId>? AttachmentIds { get; set; }
	public ICollection<TaskId>? SubtaskIds { get; set; }
}
