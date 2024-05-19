using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserConfiguration, AppUser>]
public sealed class AppUser : IdentityUser<IdentitySystemObjectId>, IEntity<IdentitySystemObjectId>
{
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[ProtectedPersonalData]
	public string? Bio { get; set; }

	[ProtectedPersonalData]
	public Language? Language { get; set; }

	//public PathString? ProfilePicturePath { get; set; }

	public ICollection<Project>? Projects { get; set; }

	public ICollection<Task>? Tasks { get; set; }
}
