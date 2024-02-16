namespace OnTrack.Backend.Api.Dto;

public sealed record class StatusDto : IDto
{
	public string Name { get; set; }
	public int Order { get; set; }
}
