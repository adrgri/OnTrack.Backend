﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<AppUserClaimConfiguration, AppUserClaim>]
public sealed class AppUserClaim : IdentityUserClaim<IdentitySystemObjectId>;
