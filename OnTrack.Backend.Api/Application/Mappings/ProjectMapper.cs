using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class ProjectMapper : StronglyTypedIdMapper<Project, ProjectId, ProjectDto>
{
	[UseMapper]
	public static AppUserMapper AppUserMapper { get; } = new();

	[UseMapper]
	public static MilestoneMapper MilestoneMapper { get; } = new();

	[MapperIgnoreTarget(nameof(Project.Id))]
	[MapProperty(nameof(ProjectDto.MilestoneIds), nameof(Project.Milestones))]
	[MapProperty(nameof(ProjectDto.MemberIds), nameof(Project.Members))]
	public override partial void ToExistingDomainModel(ProjectDto dto, Project entity);

	[MapperIgnoreSource(nameof(Project.Id))]
	[MapProperty(nameof(Project.Milestones), nameof(ProjectDto.MilestoneIds))]
	[MapProperty(nameof(Project.Members), nameof(ProjectDto.MemberIds))]
	public override partial void ToExistingDto(Project entity, ProjectDto dto);

	[MapperIgnoreTarget(nameof(Project.Id))]
	[MapProperty(nameof(ProjectDto.MilestoneIds), nameof(Project.Milestones))]
	[MapProperty(nameof(ProjectDto.MemberIds), nameof(Project.Members))]
	public override partial Project ToNewDomainModel(ProjectDto dto);

	[MapperIgnoreSource(nameof(Project.Id))]
	[MapProperty(nameof(Project.Milestones), nameof(ProjectDto.MilestoneIds))]
	[MapProperty(nameof(Project.Members), nameof(ProjectDto.MemberIds))]
	public override partial ProjectDto ToNewDto(Project entity);
}
