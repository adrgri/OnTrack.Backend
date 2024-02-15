using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<ProjectConfiguration, Project>()]
public sealed class Project : IEntity<ProjectId>
{
	public ProjectId Id { get; init; }
	public string Title { get; set; }
	public string? Description { get; set; }
	public PathString? ImagePath { get; set; }
	[MustContainAtLeastOneElement]
	public ICollection<ApplicationUser> Members { get; set; }
	public ICollection<Milestone>? Milestones { get; set; }

	public int NumberOfMilestones => Milestones?.Count ?? 0;
	public int NumberOfTasks => throw new NotImplementedException();
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ProjectId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ProjectId>))]
public sealed record class ProjectId : StronglyTypedId;

public sealed class ProjectConfiguration : EntityStronglyTypedIdConfiguration<ProjectId, Project>
{
	public override void Configure(EntityTypeBuilder<Project> builder)
	{
		base.Configure(builder);

		_ = builder.Property(project => project.ImagePath).HasConversion(
			path => path.HasValue ? path.ToString() : null,
			value => new PathString(value));
	}
}

// TODO Move this to a OnTrack.ComponentModel.DataAnnotations namespace or somewhere else
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MustContainAtLeastOneElementAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		if (value is ICollection collection)
		{
			return collection.Count is not 0;
		}

		return value is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
	}
}
