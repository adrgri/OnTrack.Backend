using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<IconConfiguration, Icon>()]
public sealed class Icon : IEntity<IconId>
{
	public IconId Id { get; init; }
	public string Name { get; set; }
	public PathString FilePath { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<IconId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<IconId>))]
public sealed record class IconId : StronglyTypedId;

public sealed class IconConfiguration : EntityStronglyTypedIdConfiguration<IconId, Icon>
{
	public override void Configure(EntityTypeBuilder<Icon> builder)
	{
		base.Configure(builder);

		_ = builder.Property(icon => icon.FilePath).HasConversion(
			path => path.HasValue ? path.ToString() : null,
			value => new PathString(value));
	}
}
