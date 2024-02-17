﻿// <auto-generated />
#nullable enable
namespace OnTrack.Backend.Api.Application.Mappings
{
    public sealed partial class ResourceMapper
    {
        public override partial void ToExistingDomainModel(global::OnTrack.Backend.Api.Dto.ResourceDto dto, global::OnTrack.Backend.Api.Models.Resource entity)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Quantity = dto.Quantity;
            entity.Unit = dto.Unit;
        }

        public override partial void ToExistingDto(global::OnTrack.Backend.Api.Models.Resource entity, global::OnTrack.Backend.Api.Dto.ResourceDto dto)
        {
            dto.Name = entity.Name;
            dto.Description = entity.Description;
            dto.Quantity = entity.Quantity;
            dto.Unit = entity.Unit;
        }

        public override partial global::OnTrack.Backend.Api.Models.Resource ToNewDomainModel(global::OnTrack.Backend.Api.Dto.ResourceDto dto)
        {
            var target = new global::OnTrack.Backend.Api.Models.Resource();
            target.Name = dto.Name;
            target.Description = dto.Description;
            target.Quantity = dto.Quantity;
            target.Unit = dto.Unit;
            return target;
        }

        public override partial global::OnTrack.Backend.Api.Dto.ResourceDto ToNewDto(global::OnTrack.Backend.Api.Models.Resource entity)
        {
            var target = new global::OnTrack.Backend.Api.Dto.ResourceDto();
            target.Name = entity.Name;
            target.Description = entity.Description;
            target.Quantity = entity.Quantity;
            target.Unit = entity.Unit;
            return target;
        }
    }
}