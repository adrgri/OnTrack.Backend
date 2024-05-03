using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class AttachmentDtoWithId : AttachmentDto, IDtoWithId<AttachmentId>
{
	public AttachmentId Id { get; set; }

	public AttachmentDtoWithId()
	{
	}

	public AttachmentDtoWithId(Attachment attachment, IMapper<AttachmentId, Attachment, AttachmentDto> mapper)
	{
		mapper.ToExistingDto(attachment, this);

		Id = attachment.Id;
	}
}
