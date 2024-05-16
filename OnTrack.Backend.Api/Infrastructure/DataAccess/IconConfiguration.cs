using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class IconConfiguration : StronglyTypedIdEntityConfiguration<IconId, Icon>
{
	//public override void Configure(EntityTypeBuilder<Icon> builder)
	//{
	//	base.Configure(builder);

	//	_ = builder.Property(icon => icon.FilePath).HasConversion(
	//		path => path.HasValue ? path.ToString() : null,
	//		value => new PathString(value));
	//}
}
