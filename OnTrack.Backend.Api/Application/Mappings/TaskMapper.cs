using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class TaskMapper : StronglyTypedIdMapper<Task, TaskId, TaskDto>
{
	[UseMapper]
	public static MilestoneMapper MilestoneMapper { get; } = new();

	[UseMapper]
	public static IconMapper IconMapper { get; } = new();

	[UseMapper]
	public static ResourceMapper ResourceMapper { get; } = new();

	[UseMapper]
	public static AttachmentMapper AttachmentMapper { get; } = new();

	[MapperIgnoreTarget(nameof(Task.Id))]
	[MapProperty(nameof(TaskDto.MilestoneId), nameof(Task.Milestone))]
	[MapProperty(nameof(TaskDto.IconId), nameof(Task.Icon))]
	[MapProperty(nameof(TaskDto.AssignedResourceIds), nameof(Task.AssignedResources))]
	[MapProperty(nameof(TaskDto.AttachmentIds), nameof(Task.Attachments))]
	[MapProperty(nameof(TaskDto.SubtaskIds), nameof(Task.Subtasks))]
	public override partial void ToExistingDomainModel(TaskDto dto, Task entity);

	[MapperIgnoreSource(nameof(Task.Id))]
	[MapProperty(nameof(Task.Milestone), nameof(TaskDto.MilestoneId))]
	[MapProperty(nameof(Task.Icon), nameof(TaskDto.IconId))]
	[MapProperty(nameof(Task.AssignedResources), nameof(TaskDto.AssignedResourceIds))]
	[MapProperty(nameof(Task.Attachments), nameof(TaskDto.AttachmentIds))]
	[MapProperty(nameof(Task.Subtasks), nameof(TaskDto.SubtaskIds))]
	public override partial void ToExistingDto(Task entity, TaskDto dto);

	[MapperIgnoreTarget(nameof(Task.Id))]
	[MapProperty(nameof(TaskDto.MilestoneId), nameof(Task.Milestone))]
	[MapProperty(nameof(TaskDto.IconId), nameof(Task.Icon))]
	[MapProperty(nameof(TaskDto.AssignedResourceIds), nameof(Task.AssignedResources))]
	[MapProperty(nameof(TaskDto.AttachmentIds), nameof(Task.Attachments))]
	[MapProperty(nameof(TaskDto.SubtaskIds), nameof(Task.Subtasks))]
	public override partial Task ToNewDomainModel(TaskDto dto);

	[MapperIgnoreSource(nameof(Task.Id))]
	[MapProperty(nameof(Task.Milestone), nameof(TaskDto.MilestoneId))]
	[MapProperty(nameof(Task.Icon), nameof(TaskDto.IconId))]
	[MapProperty(nameof(Task.AssignedResources), nameof(TaskDto.AssignedResourceIds))]
	[MapProperty(nameof(Task.Attachments), nameof(TaskDto.AttachmentIds))]
	[MapProperty(nameof(Task.Subtasks), nameof(TaskDto.SubtaskIds))]
	public override partial TaskDto ToNewDto(Task entity);
}
