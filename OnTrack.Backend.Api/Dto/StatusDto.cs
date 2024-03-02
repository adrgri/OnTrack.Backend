namespace OnTrack.Backend.Api.Dto;

public record class StatusDto : IDto
{
	public string Name { get; set; }
	public int Order { get; set; }
}
