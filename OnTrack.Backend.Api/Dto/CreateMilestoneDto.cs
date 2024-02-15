using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class CreateMilestoneDto : IDto<Milestone>
{
	public ProjectId ProjectId { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public StatusId? StatusId { get; set; }
	public ICollection<TaskId>? Tasks { get; set; }

	public Milestone ToDomainModel()
	{
		return new Milestone
		{
			Project = new Project { Id = ProjectId },
			Title = Title,
			Description = Description,
			Status = StatusId is null ? null : new Status { Id = StatusId },
			Tasks = Tasks?.Select(taskId => new Models.Task { Id = taskId }).ToList()
		};
	}
}
