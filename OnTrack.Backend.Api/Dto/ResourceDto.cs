namespace OnTrack.Backend.Api.Dto;

public sealed record class ResourceDto : IDto
{
	public string Name { get; set; }
	public string? Description { get; set; }
	public int Quantity { get; set; }
	public string Unit { get; set; }
}
