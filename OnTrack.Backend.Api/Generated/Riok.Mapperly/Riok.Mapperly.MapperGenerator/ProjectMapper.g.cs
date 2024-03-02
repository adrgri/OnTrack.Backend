﻿// <auto-generated />
#nullable enable
namespace OnTrack.Backend.Api.Application.Mappings
{
    public sealed partial class ProjectMapper
    {
        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        public override partial void ToExistingDomainModel(global::OnTrack.Backend.Api.Dto.ProjectDto dto, global::OnTrack.Backend.Api.Models.Project entity)
        {
            if (dto.MilestoneIds != null)
            {
                entity.Milestones = MapToList1(dto.MilestoneIds);
            }
            else
            {
                entity.Milestones = null;
            }
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Members = MapToList(dto.MemberIds);
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        public override partial void ToExistingDto(global::OnTrack.Backend.Api.Models.Project entity, global::OnTrack.Backend.Api.Dto.ProjectDto dto)
        {
            if (entity.Milestones != null)
            {
                dto.MilestoneIds = MapToList3(entity.Milestones);
            }
            else
            {
                dto.MilestoneIds = null;
            }
            dto.Title = entity.Title;
            dto.Description = entity.Description;
            dto.MemberIds = MapToList2(entity.Members);
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        public override partial global::OnTrack.Backend.Api.Models.Project ToNewDomainModel(global::OnTrack.Backend.Api.Dto.ProjectDto dto)
        {
            var target = new global::OnTrack.Backend.Api.Models.Project();
            if (dto.MilestoneIds != null)
            {
                target.Milestones = MapToList1(dto.MilestoneIds);
            }
            else
            {
                target.Milestones = null;
            }
            target.Title = dto.Title;
            target.Description = dto.Description;
            target.Members = MapToList(dto.MemberIds);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        public override partial global::OnTrack.Backend.Api.Dto.ProjectDto ToNewDto(global::OnTrack.Backend.Api.Models.Project entity)
        {
            var target = new global::OnTrack.Backend.Api.Dto.ProjectDto();
            if (entity.Milestones != null)
            {
                target.MilestoneIds = MapToList3(entity.Milestones);
            }
            else
            {
                target.MilestoneIds = null;
            }
            target.Title = entity.Title;
            target.Description = entity.Description;
            target.MemberIds = MapToList2(entity.Members);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        private global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.AppUser> MapToList(global::System.Collections.Generic.ICollection<global::OnTrack.Backend.Api.Models.IdentitySystemObjectId> source)
        {
            var target = new global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.AppUser>(source.Count);
            foreach (var item in source)
            {
                target.Add(AppUserMapper.FromId(item));
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        private global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.Milestone> MapToList1(global::System.Collections.Generic.ICollection<global::OnTrack.Backend.Api.Models.MilestoneId> source)
        {
            var target = new global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.Milestone>(source.Count);
            foreach (var item in source)
            {
                target.Add(MilestoneMapper.FromId(item));
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        private global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.IdentitySystemObjectId> MapToList2(global::System.Collections.Generic.ICollection<global::OnTrack.Backend.Api.Models.AppUser> source)
        {
            var target = new global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.IdentitySystemObjectId>(source.Count);
            foreach (var item in source)
            {
                target.Add(AppUserMapper.ToId(item));
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "3.4.0.0")]
        private global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.MilestoneId> MapToList3(global::System.Collections.Generic.ICollection<global::OnTrack.Backend.Api.Models.Milestone> source)
        {
            var target = new global::System.Collections.Generic.List<global::OnTrack.Backend.Api.Models.MilestoneId>(source.Count);
            foreach (var item in source)
            {
                target.Add(MilestoneMapper.ToId(item));
            }
            return target;
        }
    }
}