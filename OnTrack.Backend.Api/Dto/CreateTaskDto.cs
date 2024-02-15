using OnTrack.Backend.Api.Models;

using Task = OnTrack.Backend.Api.Models.Task;

namespace OnTrack.Backend.Api.Dto;

public class CreateTaskDto : IDto<Task>
{
	public MilestoneId MilestoneId { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? DueDate { get; set; }
	public IconId? IconId { get; set; }
	public bool IsCompleted { get; set; }
	public ICollection<ResourceId>? AssignedResources { get; set; }
	public ICollection<AttachmentId>? Attachments { get; set; }
	public ICollection<TaskId>? Subtasks { get; set; }

	public Task ToDomainModel()
	{
		return new Task
		{
			Milestone = new Milestone { Id = MilestoneId },
			Title = Title,
			Description = Description,
			StartDate = StartDate,
			DueDate = DueDate,
			Icon = IconId is null ? null : new Icon { Id = IconId },
			IsCompleted = IsCompleted,
			AssignedResources = AssignedResources?.Select(resourceId => new Resource { Id = resourceId }).ToList(),
			Attachments = Attachments?.Select(attachmentId => new Attachment { Id = attachmentId }).ToList(),
			Subtasks = Subtasks?.Select(subtaskId => new Task { Id = subtaskId }).ToList()
		};
	}
}
