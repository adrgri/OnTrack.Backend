using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AttachmentIdConfiguration, Attachment>()]
public sealed class Attachment : IEntity<AttachmentId>
{
	public AttachmentId Id { get; init; }
	public string DisplayName { get; set; }
	public PathString Path { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<AttachmentId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<AttachmentId>))]
public sealed record class AttachmentId : StronglyTypedId;

public sealed class AttachmentIdConfiguration : EntityStronglyTypedIdConfiguration<AttachmentId, Attachment>
{
	public override void Configure(EntityTypeBuilder<Attachment> builder)
	{
		base.Configure(builder);

		_ = builder.Property(attachment => attachment.Path).HasConversion(
			path => path.HasValue ? path.ToString() : null,
			value => new PathString(value));
	}
}
