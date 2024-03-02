using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class IconDtoWithId : IconDto, IDtoWithId<IconId>
{
	public IconId Id { get; set; }

	public IconDtoWithId(Icon icon, IMapper<IconId, Icon, IconDto> mapper)
	{
		mapper.ToExistingDto(icon, this);

		Id = icon.Id;
	}
}
