using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnTrack.Backend.Api.Models;

public sealed class ApplicationUser : IdentityUser //, IEntity<ApplicationUserId>
{
	// TODO Delete the ? after implementing proper identity API
	[Length(2, 20)]
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	// TODO Delete the ? after implementing proper identity API
	[Length(0, 40)]
	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[ProtectedPersonalData]
	public string FullName => $"{FirstName} {LastName}";

	[Length(0, 1_000)]
	[ProtectedPersonalData]
	public string? Bio { get; set; }

	// TODO Delete the ? after implementing proper identity API
	[ProtectedPersonalData]
	public Language? Language { get; set; }

	public PathString? ProfilePicturePath { get; set; }
}

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
	public void Configure(EntityTypeBuilder<ApplicationUser> builder)
	{
		_ = builder.Property(applicationUser => applicationUser.ProfilePicturePath).HasConversion(
			path => path.HasValue ? path.ToString() : null,
			value => new PathString(value));
	}
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<ApplicationUserId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ApplicationUserId>))]
public sealed record class ApplicationUserId : StronglyTypedId;
