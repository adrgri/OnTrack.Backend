using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfProjectsAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Project, ProjectId>(context)
	where TDbContext : DbContext;
