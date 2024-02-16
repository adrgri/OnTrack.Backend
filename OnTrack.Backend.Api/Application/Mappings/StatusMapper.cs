using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class StatusMapper : StronglyTypedIdMapper<Status, StatusId, StatusDto>
{
	[MapperIgnoreTarget(nameof(Status.Id))]
	public override partial void ToExistingDomainModel(StatusDto dto, Status entity);

	[MapperIgnoreSource(nameof(Status.Id))]
	public override partial void ToExistingDto(Status entity, StatusDto dto);

	[MapperIgnoreTarget(nameof(Status.Id))]
	public override partial Status ToNewDomainModel(StatusDto dto);

	[MapperIgnoreSource(nameof(Status.Id))]
	public override partial StatusDto ToNewDto(Status entity);
}
