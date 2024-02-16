using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserRoleConfiguration, AppUserRole>]
public sealed class AppUserRole : IdentityUserRole<IdentitySystemObjectId>;
