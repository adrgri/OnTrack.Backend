using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfResourcesAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Resource, ResourceId>(context)
	where TDbContext : DbContext;
