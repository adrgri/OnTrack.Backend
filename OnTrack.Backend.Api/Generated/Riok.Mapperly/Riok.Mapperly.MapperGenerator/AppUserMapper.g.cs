﻿// <auto-generated />
#nullable enable
namespace OnTrack.Backend.Api.Application.Mappings
{
    public sealed partial class AppUserMapper
    {
        public override partial void ToExistingDomainModel(global::OnTrack.Backend.Api.Dto.AppUserDto dto, global::OnTrack.Backend.Api.Models.AppUser entity)
        {
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Bio = dto.Bio;
            entity.Language = dto.Language;
            entity.ProfilePicturePath = dto.ProfilePicturePath;
            entity.UserName = dto.UserName;
            entity.Email = dto.Email;
            entity.EmailConfirmed = dto.EmailConfirmed;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.PhoneNumberConfirmed = dto.PhoneNumberConfirmed;
            entity.TwoFactorEnabled = dto.TwoFactorEnabled;
            entity.LockoutEnd = dto.LockoutEnd;
            entity.LockoutEnabled = dto.LockoutEnabled;
            entity.AccessFailedCount = dto.AccessFailedCount;
        }

        public override partial void ToExistingDto(global::OnTrack.Backend.Api.Models.AppUser entity, global::OnTrack.Backend.Api.Dto.AppUserDto dto)
        {
            dto.UserName = entity.UserName;
            dto.Email = entity.Email;
            dto.FirstName = entity.FirstName;
            dto.LastName = entity.LastName;
            dto.Bio = entity.Bio;
            dto.Language = entity.Language;
            dto.ProfilePicturePath = entity.ProfilePicturePath;
            dto.EmailConfirmed = entity.EmailConfirmed;
            dto.PhoneNumber = entity.PhoneNumber;
            dto.PhoneNumberConfirmed = entity.PhoneNumberConfirmed;
            dto.TwoFactorEnabled = entity.TwoFactorEnabled;
            dto.LockoutEnd = entity.LockoutEnd;
            dto.LockoutEnabled = entity.LockoutEnabled;
            dto.AccessFailedCount = entity.AccessFailedCount;
        }

        public override partial global::OnTrack.Backend.Api.Models.AppUser ToNewDomainModel(global::OnTrack.Backend.Api.Dto.AppUserDto dto)
        {
            var target = new global::OnTrack.Backend.Api.Models.AppUser();
            target.FirstName = dto.FirstName;
            target.LastName = dto.LastName;
            target.Bio = dto.Bio;
            target.Language = dto.Language;
            target.ProfilePicturePath = dto.ProfilePicturePath;
            target.UserName = dto.UserName;
            target.Email = dto.Email;
            target.EmailConfirmed = dto.EmailConfirmed;
            target.PhoneNumber = dto.PhoneNumber;
            target.PhoneNumberConfirmed = dto.PhoneNumberConfirmed;
            target.TwoFactorEnabled = dto.TwoFactorEnabled;
            target.LockoutEnd = dto.LockoutEnd;
            target.LockoutEnabled = dto.LockoutEnabled;
            target.AccessFailedCount = dto.AccessFailedCount;
            return target;
        }

        public override partial global::OnTrack.Backend.Api.Dto.AppUserDto ToNewDto(global::OnTrack.Backend.Api.Models.AppUser entity)
        {
            var target = new global::OnTrack.Backend.Api.Dto.AppUserDto();
            target.UserName = entity.UserName;
            target.Email = entity.Email;
            target.FirstName = entity.FirstName;
            target.LastName = entity.LastName;
            target.Bio = entity.Bio;
            target.Language = entity.Language;
            target.ProfilePicturePath = entity.ProfilePicturePath;
            target.EmailConfirmed = entity.EmailConfirmed;
            target.PhoneNumber = entity.PhoneNumber;
            target.PhoneNumberConfirmed = entity.PhoneNumberConfirmed;
            target.TwoFactorEnabled = entity.TwoFactorEnabled;
            target.LockoutEnd = entity.LockoutEnd;
            target.LockoutEnabled = entity.LockoutEnabled;
            target.AccessFailedCount = entity.AccessFailedCount;
            return target;
        }
    }
}