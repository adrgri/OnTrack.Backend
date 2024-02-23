using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.DataAccess;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<AppUser, AppRole, IdentitySystemObjectId, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>(options)
{
	public DbSet<Language> Languages { get; set; }
	public DbSet<Project> Projects { get; set; }
	public DbSet<Milestone> Milestones { get; set; }
	public DbSet<Task> Tasks { get; set; }
	public DbSet<Resource> Resources { get; set; }
	public DbSet<Attachment> Attachments { get; set; }
	public DbSet<Status> Statuses { get; set; }
	public DbSet<Icon> Icons { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		_ = builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
