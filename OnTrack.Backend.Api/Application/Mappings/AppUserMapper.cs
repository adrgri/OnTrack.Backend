using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class AppUserMapper : StronglyTypedIdMapper<AppUser, IdentitySystemObjectId, AppUserDto>
{
	[MapperIgnoreTarget(nameof(AppUser.Id))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	public override partial void ToExistingDomainModel(AppUserDto dto, AppUser entity);

	[MapperIgnoreSource(nameof(AppUser.Id))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapperIgnoreSource(nameof(AppUser.Id))]
	public override partial void ToExistingDto(AppUser entity, AppUserDto dto);

	[MapperIgnoreTarget(nameof(AppUser.Id))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	public override partial AppUser ToNewDomainModel(AppUserDto dto);

	[MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapperIgnoreSource(nameof(AppUser.Id))]
	public override partial AppUserDto ToNewDto(AppUser entity);
}
