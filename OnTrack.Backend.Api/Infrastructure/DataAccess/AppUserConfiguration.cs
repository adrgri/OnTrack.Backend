using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

public sealed class AppUserConfiguration : StronglyTypedIdEntityConfiguration<IdentitySystemObjectId, AppUser>
{
	//public override void Configure(EntityTypeBuilder<AppUser> builder)
	//{
	//	base.Configure(builder);

	//	_ = builder.Property(applicationUser => applicationUser.ProfilePicturePath).HasConversion(
	//		path => path.HasValue ? path.ToString() : null,
	//		value => new PathString(value));
	//}
}
