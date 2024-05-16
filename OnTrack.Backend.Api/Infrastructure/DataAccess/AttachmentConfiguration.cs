using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class AttachmentConfiguration : StronglyTypedIdEntityConfiguration<AttachmentId, Attachment>
{
	//public override void Configure(EntityTypeBuilder<Attachment> builder)
	//{
	//	base.Configure(builder);

	//	_ = builder.Property(attachment => attachment.Path).HasConversion(
	//		path => path.HasValue ? path.ToString() : null,
	//		value => new PathString(value));
	//}
}
