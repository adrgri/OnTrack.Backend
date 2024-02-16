using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserTokenConfiguration, AppUserToken>]
public sealed class AppUserToken : IdentityUserToken<IdentitySystemObjectId>;
