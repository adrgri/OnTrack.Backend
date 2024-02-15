﻿using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<EntityStronglyTypedIdConfiguration<MilestoneId, Milestone>, Milestone>()]
public sealed class Milestone : IEntity<MilestoneId>
{
	public MilestoneId Id { get; init; }
	public Project Project { get; set; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public Status? Status { get; set; }
	public ICollection<Task>? Tasks { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<MilestoneId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<MilestoneId>))]
public sealed record class MilestoneId : StronglyTypedId;

//public sealed class MilestoneConfiguration : EntityStronglyTypedIdConfiguration<MilestoneId, Milestone>;
