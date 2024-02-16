using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfStatusesAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Status, StatusId>(context)
	where TDbContext : DbContext;
