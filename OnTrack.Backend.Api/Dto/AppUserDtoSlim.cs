using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class AppUserDtoSlim : IDto
{
	public IdentitySystemObjectId Id { get; set; }

	[ProtectedPersonalData]
	public string? UserName { get; set; }

	[Length(2, 20)]
	[ProtectedPersonalData]
	public string? FirstName { get; set; }

	[Length(0, 40)]
	[ProtectedPersonalData]
	public string? LastName { get; set; }

	[Length(0, 1_000)]
	[ProtectedPersonalData]
	public string? Bio { get; set; }

	public AppUserDtoSlim()
	{
	}

	public AppUserDtoSlim(AppUser user, IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim> mapper)
	{
		mapper.ToExistingDto(user, this);

		Id = user.Id;
	}
}
