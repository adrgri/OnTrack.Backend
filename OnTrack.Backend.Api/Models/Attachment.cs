using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AttachmentConfiguration, Attachment>]
public sealed record class Attachment : Entity<AttachmentId>
{
	public string DisplayName { get; set; }
	//public PathString Path { get; set; }
}
