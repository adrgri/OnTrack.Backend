namespace OnTrack.Backend.Api.Dto;

public record class ResourceDto : IDto
{
	public string Name { get; set; }
	public string? Description { get; set; }
	public int Quantity { get; set; }
	public string Unit { get; set; }
}
