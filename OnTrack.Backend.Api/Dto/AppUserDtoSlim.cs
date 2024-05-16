using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace OnTrack.Backend.Api.Dto;

public record class AppUserDtoSlim : IDto
{
	// TODO: Removed username because setting it to null caused the built-in identity api to throw null reference exception upon login
	//[ProtectedPersonalData]
	//public string? UserName { get; set; }

	[Length(2, 20)]
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	[Length(0, 40)]
	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[Length(0, 1_000)]
	[ProtectedPersonalData]
	public string? Bio { get; set; }

	//[ProtectedPersonalData]
	//public LanguageId? LanguageId { get; set; }

	//public PathString? ProfilePicturePath { get; set; }
}
