using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class ProjectConfiguration : StronglyTypedIdEntityConfiguration<ProjectId, Project>
{
	//public override void Configure(EntityTypeBuilder<Project> builder)
	//{
	//	base.Configure(builder);

	//	_ = builder.Property(project => project.ImagePath).HasConversion(
	//		path => path.HasValue ? path.ToString() : null,
	//		value => new PathString(value));
	//}
}
