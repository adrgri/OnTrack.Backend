using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class StatusDtoWithId : StatusDto, IDtoWithId<StatusId>
{
	public StatusId Id { get; set; }

	public StatusDtoWithId()
	{
		
	}

	public StatusDtoWithId(Status status, IMapper<StatusId, Status, StatusDto> mapper)
	{
		mapper.ToExistingDto(status, this);

		Id = status.Id;
	}
}
