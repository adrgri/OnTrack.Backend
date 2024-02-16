using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

using Task = System.Threading.Tasks.Task;

namespace OnTrack.Backend.Api.Services;

public sealed class EfAppUsersAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, AppUser, IdentitySystemObjectId>(context)
	where TDbContext : DbContext
{
	private const string _template = "Please use the identity API endpoints in order to {0} user.";

	public override Task Add(AppUser entity)
	{
		throw new NotSupportedException(string.Format(_template, "register a new"));
	}

	public override Task Remove(AppUser entity)
	{
		// TODO: Implement endpoint to remove a user from the identity system
		throw new NotImplementedException();
		throw new NotSupportedException(string.Format(_template, "remove a"));
	}
}
