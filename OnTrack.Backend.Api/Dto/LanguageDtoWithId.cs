using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class LanguageDtoWithId : LanguageDto, IDtoWithId<LanguageId>
{
	public LanguageId Id { get; set; }

	public LanguageDtoWithId(Language language, IMapper<LanguageId, Language, LanguageDto> mapper)
	{
		mapper.ToExistingDto(language, this);

		Id = language.Id;
	}
}
