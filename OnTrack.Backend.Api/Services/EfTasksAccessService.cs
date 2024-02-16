using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

using Task = OnTrack.Backend.Api.Models.Task;

namespace OnTrack.Backend.Api.Services;

public sealed class EfTasksAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Task, TaskId>(context)
	where TDbContext : DbContext;
