using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfAppUsersAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<AppUser, IdentitySystemObjectId, TDbContext>(context)
	where TDbContext : DbContext
{
	private const string _template = "Please use the identity API endpoints in order to {0} user.";

	public override SysTask Add(AppUser entity, CancellationToken cancellationToken)
	{
		throw new NotSupportedException(string.Format(_template, "register new"));
	}

	public override SysTask Remove(AppUser entity, CancellationToken cancellationToken)
	{
		// TODO: Implement endpoint to remove a user from the identity system
		throw new NotImplementedException();
		throw new NotSupportedException(string.Format(_template, "remove existing"));
	}
}
