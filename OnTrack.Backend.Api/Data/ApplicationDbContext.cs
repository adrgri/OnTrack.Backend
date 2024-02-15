using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

using Task = OnTrack.Backend.Api.Models.Task;

namespace OnTrack.Backend.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	: IdentityDbContext<ApplicationUser>(options)
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

		_ = builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
	}
}
