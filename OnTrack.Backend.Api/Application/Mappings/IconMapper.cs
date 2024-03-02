using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class IconMapper : StronglyTypedIdMapper<IconId, Icon, IconDto>
{
	[MapperIgnoreTarget(nameof(Icon.Id))]
	public override partial void ToExistingDomainModel(IconDto dto, Icon entity);

	[MapperIgnoreSource(nameof(Icon.Id))]
	public override partial void ToExistingDto(Icon entity, IconDto dto);

	[MapperIgnoreTarget(nameof(Icon.Id))]
	public override partial Icon ToNewDomainModel(IconDto dto);

	[MapperIgnoreSource(nameof(Icon.Id))]
	public override partial IconDto ToNewDto(Icon entity);
}
