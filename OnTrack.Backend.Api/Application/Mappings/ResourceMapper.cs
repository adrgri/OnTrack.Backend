using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class ResourceMapper : StronglyTypedIdMapper<Resource, ResourceId, ResourceDto>
{
	[MapperIgnoreTarget(nameof(Resource.Id))]
	public override partial void ToExistingDomainModel(ResourceDto dto, Resource entity);

	[MapperIgnoreSource(nameof(Resource.Id))]
	public override partial void ToExistingDto(Resource entity, ResourceDto dto);

	[MapperIgnoreTarget(nameof(Resource.Id))]
	public override partial Resource ToNewDomainModel(ResourceDto dto);

	[MapperIgnoreSource(nameof(Resource.Id))]
	public override partial ResourceDto ToNewDto(Resource entity);
}
