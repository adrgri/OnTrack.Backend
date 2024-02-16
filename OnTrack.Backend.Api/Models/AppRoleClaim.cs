using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppRoleClaimConfiguration, AppRoleClaim>]
public sealed class AppRoleClaim : IdentityRoleClaim<IdentitySystemObjectId>;
