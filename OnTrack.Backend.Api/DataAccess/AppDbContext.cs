using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.DataAccess;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<AppUser, AppRole, IdentitySystemObjectId, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>(options)
{
	public DbSet<Language> Languages { get; set; }
	public DbSet<Project> Projects { get; set; }
	public DbSet<Task> Tasks { get; set; }
	public DbSet<Resource> Resources { get; set; }
	public DbSet<Attachment> Attachments { get; set; }
	public DbSet<Status> Statuses { get; set; }
	public DbSet<Icon> Icons { get; set; }

	private static void ConfigureTasks(ModelBuilder builder)
	{
		// Create shadow properties for each self-referencing collection
		builder.Entity<Task>()
			.Property<TaskId>("PredecessorId")
			.IsRequired(false);

		builder.Entity<Task>()
			.Property<TaskId>("SuccessorId")
			.IsRequired(false);

		builder.Entity<Task>()
			.Property<TaskId>("SubtaskId")
			.IsRequired(false);

		// Establish relationships between the collections and their respective shadow properties
		builder.Entity<Task>()
			.HasMany(task => task.Predecessors)
			.WithOne()
			.HasForeignKey("PredecessorId");

		builder.Entity<Task>()
			.HasMany(task => task.Successors)
			.WithOne()
			.HasForeignKey("SuccessorId");

		builder.Entity<Task>()
			.HasMany(task => task.Subtasks)
			.WithOne()
			.HasForeignKey("SubtaskId");
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		_ = builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

		ConfigureTasks(builder);
	}
}
