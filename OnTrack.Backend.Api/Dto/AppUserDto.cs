using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public record class AppUserDto : IDto
{
	[ProtectedPersonalData]
	public string? UserName { get; set; }

	[EmailAddress]
	[ProtectedPersonalData]
	public string? Email { get; set; }

	// TODO Delete the ? after implementing proper identity API
	[Length(2, 20)]
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	// TODO Delete the ? after implementing proper identity API
	[Length(0, 40)]
	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[Length(0, 1_000)]
	[ProtectedPersonalData]
	public string? Bio { get; set; }

	// TODO Delete the ? after implementing proper identity API
	[ProtectedPersonalData]
	public Language? Language { get; set; }

	//public PathString? ProfilePicturePath { get; set; }

	public ICollection<ProjectId>? ProjectIds { get; set; }

	public ICollection<TaskId>? TaskIds { get; set; }

	[PersonalData]
	public bool EmailConfirmed { get; set; }

	[Phone]
	[ProtectedPersonalData]
	public string? PhoneNumber { get; set; }

	[PersonalData]
	public bool PhoneNumberConfirmed { get; set; }

	[PersonalData]
	public bool TwoFactorEnabled { get; set; }

	public DateTimeOffset? LockoutEnd { get; set; }

	public bool LockoutEnabled { get; set; }

	public int AccessFailedCount { get; set; }
}
