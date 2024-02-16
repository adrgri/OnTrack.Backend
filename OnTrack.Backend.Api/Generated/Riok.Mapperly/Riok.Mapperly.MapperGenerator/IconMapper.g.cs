﻿// <auto-generated />
#nullable enable
namespace OnTrack.Backend.Api.Application.Mappings
{
    public sealed partial class IconMapper
    {
        public override partial void ToExistingDomainModel(global::OnTrack.Backend.Api.Dto.IconDto dto, global::OnTrack.Backend.Api.Models.Icon entity)
        {
            entity.Name = dto.Name;
            entity.FilePath = dto.FilePath;
        }

        public override partial void ToExistingDto(global::OnTrack.Backend.Api.Models.Icon entity, global::OnTrack.Backend.Api.Dto.IconDto dto)
        {
            dto.Name = entity.Name;
            dto.FilePath = entity.FilePath;
        }

        public override partial global::OnTrack.Backend.Api.Models.Icon ToNewDomainModel(global::OnTrack.Backend.Api.Dto.IconDto dto)
        {
            var target = new global::OnTrack.Backend.Api.Models.Icon();
            target.Name = dto.Name;
            target.FilePath = dto.FilePath;
            return target;
        }

        public override partial global::OnTrack.Backend.Api.Dto.IconDto ToNewDto(global::OnTrack.Backend.Api.Models.Icon entity)
        {
            var target = new global::OnTrack.Backend.Api.Dto.IconDto();
            target.Name = entity.Name;
            target.FilePath = entity.FilePath;
            return target;
        }
    }
}