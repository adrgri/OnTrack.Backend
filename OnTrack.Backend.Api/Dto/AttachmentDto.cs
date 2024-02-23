namespace OnTrack.Backend.Api.Dto;

public sealed record class AttachmentDto : IDto
{
	public string DisplayName { get; set; }
	//public PathString Path { get; set; }
}
