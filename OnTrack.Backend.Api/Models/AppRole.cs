using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<IdentitySystemObjectId, AppRole>, AppRole>]
public sealed class AppRole : IdentityRole<IdentitySystemObjectId>, IEntity<IdentitySystemObjectId>;
