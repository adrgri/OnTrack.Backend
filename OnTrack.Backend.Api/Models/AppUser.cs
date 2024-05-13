using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserConfiguration, AppUser>]
public sealed class AppUser : IdentityUser<IdentitySystemObjectId>, IEntity<IdentitySystemObjectId>
{
	[Length(2, 20)]
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	[Length(0, 40)]
	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[Length(0, 1_000)]
	[ProtectedPersonalData]
	public string? Bio { get; set; }

	[ProtectedPersonalData]
	public Language? Language { get; set; }

	//public PathString? ProfilePicturePath { get; set; }

	public ICollection<Project>? Projects { get; set; }

	public ICollection<Task>? Tasks { get; set; }
}
