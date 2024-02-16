using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AttachmentConfiguration, Attachment>]
public sealed record class Attachment : IEntity<AttachmentId>
{
	public AttachmentId Id { get; set; }
	public string DisplayName { get; set; }
	public PathString Path { get; set; }
}
