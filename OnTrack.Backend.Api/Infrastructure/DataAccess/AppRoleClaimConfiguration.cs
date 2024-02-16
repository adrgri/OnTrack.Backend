using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class AppRoleClaimConfiguration : IEntityTypeConfiguration<AppRoleClaim>
{
	public void Configure(EntityTypeBuilder<AppRoleClaim> builder)
	{
		_ = builder.Property(entity => entity.RoleId)
			.HasConversion(IStronglyTypedIdConverter<IdentitySystemObjectId>.IdConverter);
	}
}
