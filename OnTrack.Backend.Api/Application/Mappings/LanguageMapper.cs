using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class LanguageMapper : StronglyTypedIdMapper<LanguageId, Language, LanguageDto>
{
	[MapperIgnoreTarget(nameof(Language.Id))]
	public override partial void ToExistingDomainModel(LanguageDto dto, Language entity);

	[MapperIgnoreSource(nameof(Language.Id))]
	public override partial void ToExistingDto(Language entity, LanguageDto dto);

	[MapperIgnoreTarget(nameof(Language.Id))]
	public override partial Language ToNewDomainModel(LanguageDto dto);

	[MapperIgnoreSource(nameof(Language.Id))]
	public override partial LanguageDto ToNewDto(Language entity);
}
