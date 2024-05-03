using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class AppUserDtoWithId : AppUserDto, IDtoWithId<IdentitySystemObjectId>
{
	public IdentitySystemObjectId Id { get; set; }

	public AppUserDtoWithId()
	{
	}

	public AppUserDtoWithId(AppUser user, IMapper<IdentitySystemObjectId, AppUser, AppUserDto> mapper)
	{
		mapper.ToExistingDto(user, this);

		Id = user.Id;
	}
}
