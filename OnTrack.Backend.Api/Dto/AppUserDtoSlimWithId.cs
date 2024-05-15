using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class AppUserDtoSlimWithId : AppUserDtoSlim, IDtoWithId<IdentitySystemObjectId>
{
	public IdentitySystemObjectId Id { get; set; }

	public AppUserDtoSlimWithId()
	{
	}

	public AppUserDtoSlimWithId(AppUser user, IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim> mapper)
	{
		mapper.ToExistingDto(user, this);

		Id = user.Id;
	}
}
