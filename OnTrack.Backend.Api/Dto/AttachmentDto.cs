namespace OnTrack.Backend.Api.Dto;

public record class AttachmentDto : IDto
{
	public string DisplayName { get; set; }
	//public PathString Path { get; set; }
}
