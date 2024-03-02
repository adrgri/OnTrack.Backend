using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class ResourceDtoWithId : ResourceDto, IDtoWithId<ResourceId>
{
	public ResourceId Id { get; set; }

	public ResourceDtoWithId(Resource resource, IMapper<ResourceId, Resource, ResourceDto> mapper)
	{
		mapper.ToExistingDto(resource, this);

		Id = resource.Id;
	}
}
