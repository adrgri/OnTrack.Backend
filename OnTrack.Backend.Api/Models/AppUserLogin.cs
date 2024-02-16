using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserLoginConfiguration, AppUserLogin>]
public sealed class AppUserLogin : IdentityUserLogin<IdentitySystemObjectId>;
