using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

using Riok.Mapperly.Abstractions;

namespace OnTrack.Backend.Api.Application.Mappings;

[Mapper]
public sealed partial class AppUserMapper : StronglyTypedIdMapper<IdentitySystemObjectId, AppUser, AppUserDto>, IStronglyTypedIdMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim>
{
	[UseMapper]
	public static ProjectMapper ProjectMapper { get; } = new();

	[UseMapper]
	public static TaskMapper TaskMapper { get; } = new();

	[MapperIgnoreTarget(nameof(AppUser.Id)), MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail)), MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp)), MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUserDto.ProjectIds), nameof(AppUser.Projects))]
	[MapProperty(nameof(AppUserDto.TaskIds), nameof(AppUser.Tasks))]
	public override partial void ToExistingDomainModel(AppUserDto dto, AppUser entity);

	// TODO: Trochę to kiepsko wygląda, ale na razie niech będzie tak, i tak mapper za chwilę będzie inaczej skonstruowany
	public void ToExistingDomainModel(AppUserDtoSlim dto, AppUser entity)
	{
		throw new NotSupportedException();
	}

	[MapperIgnoreSource(nameof(AppUser.Id)), MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail)), MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp)), MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUser.Projects), nameof(AppUserDto.ProjectIds))]
	[MapProperty(nameof(AppUser.Tasks), nameof(AppUserDto.TaskIds))]
	public override partial void ToExistingDto(AppUser entity, AppUserDto dto);

	// I didn't bother to fix warning RMG020 because this mapper will be changed anyway
	public partial void ToExistingDto(AppUser entity, AppUserDtoSlim dto);

	[MapperIgnoreTarget(nameof(AppUser.Id)), MapperIgnoreTarget(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreTarget(nameof(AppUser.NormalizedEmail)), MapperIgnoreTarget(nameof(AppUser.PasswordHash))]
	[MapperIgnoreTarget(nameof(AppUser.SecurityStamp)), MapperIgnoreTarget(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUserDto.ProjectIds), nameof(AppUser.Projects))]
	[MapProperty(nameof(AppUserDto.TaskIds), nameof(AppUser.Tasks))]
	public override partial AppUser ToNewDomainModel(AppUserDto dto);

	// TODO: Tu to samo
	public AppUser ToNewDomainModel(AppUserDtoSlim dto)
	{
		throw new NotSupportedException();
	}

	[MapperIgnoreSource(nameof(AppUser.Id)), MapperIgnoreSource(nameof(AppUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(AppUser.NormalizedEmail)), MapperIgnoreSource(nameof(AppUser.PasswordHash))]
	[MapperIgnoreSource(nameof(AppUser.SecurityStamp)), MapperIgnoreSource(nameof(AppUser.ConcurrencyStamp))]
	[MapProperty(nameof(AppUser.Projects), nameof(AppUserDto.ProjectIds))]
	[MapProperty(nameof(AppUser.Tasks), nameof(AppUserDto.TaskIds))]
	public override partial AppUserDto ToNewDto(AppUser entity);

	public partial AppUserDtoSlim ToNewDtoSlim(AppUser entity);

	// TODO: Auć
	AppUserDtoSlim IMapper<IdentitySystemObjectId, AppUser, AppUserDtoSlim>.ToNewDto(AppUser entity)
	{
		return ToNewDtoSlim(entity);
	}
}
