namespace OnTrack.Backend.Api.Dto;

public record class LanguageDto : IDto
{
	public string Code { get; set; }
	public string Name { get; set; }
}
