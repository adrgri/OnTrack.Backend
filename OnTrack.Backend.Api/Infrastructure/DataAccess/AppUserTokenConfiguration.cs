using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class AppUserTokenConfiguration : IEntityTypeConfiguration<AppUserToken>
{
	public void Configure(EntityTypeBuilder<AppUserToken> builder)
	{
		_ = builder.Property(entity => entity.UserId)
			.HasConversion(IStronglyTypedIdConverter<IdentitySystemObjectId>.IdConverter);
	}
}
