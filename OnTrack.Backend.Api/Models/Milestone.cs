using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

// TODO: This attribute should not be on the model level, since this will be domain thing, domain object can not have dependencies on the infrastructure, move it to the infrastructure project as a configuration class
[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<MilestoneId, Milestone>, Milestone>]
public sealed record class Milestone : IEntity<MilestoneId>
{
	public MilestoneId Id { get; set; }
	public Project Project { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public Status? Status { get; set; }
	public ICollection<Task>? Tasks { get; set; }
}
