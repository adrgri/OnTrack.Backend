using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class MilestoneMapper : StronglyTypedIdMapper<MilestoneId, Milestone, MilestoneDto>
{
	[UseMapper]
	public static ProjectMapper ProjectMapper { get; } = new();

	[UseMapper]
	public static StatusMapper StatusMapper { get; } = new();

	[UseMapper]
	public static TaskMapper TaskMapper { get; } = new();

	[MapperIgnoreTarget(nameof(Milestone.Id))]
	[MapProperty(nameof(MilestoneDto.ProjectId), nameof(Milestone.Project))]
	[MapProperty(nameof(MilestoneDto.StatusId), nameof(Milestone.Status))]
	[MapProperty(nameof(MilestoneDto.TaskIds), nameof(Milestone.Tasks))]
	public override partial void ToExistingDomainModel(MilestoneDto dto, Milestone entity);

	[MapperIgnoreSource(nameof(Milestone.Id))]
	[MapProperty(nameof(Milestone.Project), nameof(MilestoneDto.ProjectId))]
	[MapProperty(nameof(Milestone.Status), nameof(MilestoneDto.StatusId))]
	[MapProperty(nameof(Milestone.Tasks), nameof(MilestoneDto.TaskIds))]
	public override partial void ToExistingDto(Milestone entity, MilestoneDto dto);

	[MapperIgnoreTarget(nameof(Milestone.Id))]
	[MapProperty(nameof(MilestoneDto.ProjectId), nameof(Milestone.Project))]
	[MapProperty(nameof(MilestoneDto.StatusId), nameof(Milestone.Status))]
	[MapProperty(nameof(MilestoneDto.TaskIds), nameof(Milestone.Tasks))]
	public override partial Milestone ToNewDomainModel(MilestoneDto dto);

	[MapperIgnoreSource(nameof(Milestone.Id))]
	[MapProperty(nameof(Milestone.Project), nameof(MilestoneDto.ProjectId))]
	[MapProperty(nameof(Milestone.Status), nameof(MilestoneDto.StatusId))]
	[MapProperty(nameof(Milestone.Tasks), nameof(MilestoneDto.TaskIds))]
	public override partial MilestoneDto ToNewDto(Milestone entity);
}
