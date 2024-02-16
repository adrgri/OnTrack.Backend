using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfIconsAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Icon, IconId>(context)
	where TDbContext : DbContext;
