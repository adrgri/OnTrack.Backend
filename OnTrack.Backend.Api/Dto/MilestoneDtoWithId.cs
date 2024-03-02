using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class MilestoneDtoWithId : MilestoneDto, IDtoWithId<MilestoneId>
{
	public MilestoneId Id { get; set; }

	public MilestoneDtoWithId(Milestone milestone, IMapper<MilestoneId, Milestone, MilestoneDto> mapper)
	{
		mapper.ToExistingDto(milestone, this);

		Id = milestone.Id;
	}
}
