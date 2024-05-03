using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class ProjectMapper : StronglyTypedIdMapper<ProjectId, Project, ProjectDto>
{
	[UseMapper]
	public static AppUserMapper AppUserMapper { get; } = new();

	[UseMapper]
	public static TaskMapper TaskMapper { get; } = new();

	[MapperIgnoreTarget(nameof(Project.Id))]
	[MapProperty(nameof(ProjectDto.TaskIds), nameof(Project.Tasks))]
	[MapProperty(nameof(ProjectDto.MemberIds), nameof(Project.Members))]
	public override partial void ToExistingDomainModel(ProjectDto dto, Project entity);

	[MapperIgnoreSource(nameof(Project.Id))]
	[MapProperty(nameof(Project.Tasks), nameof(ProjectDto.TaskIds))]
	[MapProperty(nameof(Project.Members), nameof(ProjectDto.MemberIds))]
	public override partial void ToExistingDto(Project entity, ProjectDto dto);

	[MapperIgnoreTarget(nameof(Project.Id))]
	[MapProperty(nameof(ProjectDto.TaskIds), nameof(Project.Tasks))]
	[MapProperty(nameof(ProjectDto.MemberIds), nameof(Project.Members))]
	public override partial Project ToNewDomainModel(ProjectDto dto);

	[MapperIgnoreSource(nameof(Project.Id))]
	[MapProperty(nameof(Project.Tasks), nameof(ProjectDto.TaskIds))]
	[MapProperty(nameof(Project.Members), nameof(ProjectDto.MemberIds))]
	public override partial ProjectDto ToNewDto(Project entity);
}
