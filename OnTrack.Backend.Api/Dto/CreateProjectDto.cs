using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class CreateProjectDto : IDto<Project>
{
	public string Title { get; set; }
	public string? Description { get; set; }
	public PathString? ImagePath { get; set; }
	[MustContainAtLeastOneElement]
	public ICollection<ApplicationUser> Members { get; set; }
	public ICollection<MilestoneId>? Milestones { get; set; }

	public Project ToDomainModel()
	{
		return new Project
		{
			Title = Title,
			Description = Description,
			ImagePath = ImagePath,
			Members = Members,
			Milestones = Milestones?.Select(milestoneId => new Milestone { Id = milestoneId }).ToList(),
		};
	}
}
