using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
{
	public void Configure(EntityTypeBuilder<AppUserRole> builder)
	{
		_ = builder.Property(entity => entity.UserId)
			.HasConversion(IStronglyTypedIdConverter<IdentitySystemObjectId>.IdConverter);

		_ = builder.Property(entity => entity.RoleId)
			.HasConversion(IStronglyTypedIdConverter<IdentitySystemObjectId>.IdConverter);
	}
}
