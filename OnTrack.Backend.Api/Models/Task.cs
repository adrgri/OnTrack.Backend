using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<TaskId, Task>, Task>()]
public sealed record class Task : IEntity<TaskId>
{
	public TaskId Id { get; init; }
	public Milestone Milestone { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? DueDate { get; set; }
	public Icon? Icon { get; set; }
	public bool IsCompleted { get; set; }
	public ICollection<Resource>? AssignedResources { get; set; }
	public ICollection<Attachment>? Attachments { get; set; }
	public ICollection<Task>? Subtasks { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<TaskId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<TaskId>))]
public sealed record class TaskId : StronglyTypedId;
