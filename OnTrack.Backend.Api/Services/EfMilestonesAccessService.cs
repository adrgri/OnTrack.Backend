using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfMilestonesAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Milestone, MilestoneId>(context)
	where TDbContext : DbContext;
