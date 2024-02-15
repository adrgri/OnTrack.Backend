using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public class CreateLanguageDto : IDto<Language>
{
	public string Code { get; set; }
	public string Name { get; set; }

	public Language ToDomainModel()
	{
		return new Language()
		{
			Code = Code,
			Name = Name
		};
	}
}
