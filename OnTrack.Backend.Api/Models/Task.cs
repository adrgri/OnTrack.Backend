﻿using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

// TODO: Zmień nazwę tej klasy na coś innego, bo jest konflikt z klasą Task z namespace'a System.Threading.Tasks :/
[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<TaskId, Task>, Task>]
public sealed record class Task : Entity<TaskId>
{
	public Project Project { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? DueDate { get; set; }
	public Status? Status { get; set; }
	public Icon? Icon { get; set; }
	public bool IsCompleted { get; set; }
	public ICollection<Task>? Predecessors { get; set; }
	public ICollection<Task>? Successors { get; set; }
	public ICollection<AppUser>? AssignedMembers { get; set; }
	public ICollection<Resource>? AssignedResources { get; set; }
	public ICollection<Attachment>? Attachments { get; set; }
	public ICollection<Task>? Subtasks { get; set; }
}
