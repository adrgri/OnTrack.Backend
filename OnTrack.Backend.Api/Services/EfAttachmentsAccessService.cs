using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfAttachmentsAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Attachment, AttachmentId>(context)
	where TDbContext : DbContext;
