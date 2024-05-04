using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class AppUserMapper : StronglyTypedIdMapper<IdentitySystemObjectId, AppUser, AppUserDto>
{
	[UseMapper]
	public static ProjectMapper ProjectMapper { get; } = new();

	[UseMapper]
	public static TaskMapper TaskMapper { get; } = new();

	[MapperIgnoreTarget(nameof(AppUser.Id))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUserDto.ProjectIds), nameof(AppUser.Projects))]
	[MapProperty(nameof(AppUserDto.TaskIds), nameof(AppUser.Tasks))]
	public override partial void ToExistingDomainModel(AppUserDto dto, AppUser entity);

	[MapperIgnoreSource(nameof(AppUser.Id))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUser.Projects), nameof(AppUserDto.ProjectIds))]
	[MapProperty(nameof(AppUser.Tasks), nameof(AppUserDto.TaskIds))]
	public override partial void ToExistingDto(AppUser entity, AppUserDto dto);

	[MapperIgnoreTarget(nameof(AppUser.Id))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUserDto.ProjectIds), nameof(AppUser.Projects))]
	[MapProperty(nameof(AppUserDto.TaskIds), nameof(AppUser.Tasks))]
	public override partial AppUser ToNewDomainModel(AppUserDto dto);

	[MapperIgnoreSource(nameof(AppUser.Id))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUser.Projects), nameof(AppUserDto.ProjectIds))]
	[MapProperty(nameof(AppUser.Tasks), nameof(AppUserDto.TaskIds))]
	public override partial AppUserDto ToNewDto(AppUser entity);
}
