using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class AttachmentMapper : StronglyTypedIdMapper<AttachmentId, Attachment, AttachmentDto>
{
	[MapperIgnoreTarget(nameof(Attachment.Id))]
	public override partial void ToExistingDomainModel(AttachmentDto dto, Attachment entity);

	[MapperIgnoreSource(nameof(Attachment.Id))]
	public override partial void ToExistingDto(Attachment entity, AttachmentDto dto);

	[MapperIgnoreTarget(nameof(Attachment.Id))]
	public override partial Attachment ToNewDomainModel(AttachmentDto dto);

	[MapperIgnoreSource(nameof(Attachment.Id))]
	public override partial AttachmentDto ToNewDto(Attachment entity);
}
