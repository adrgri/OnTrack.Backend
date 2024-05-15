using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class ProjectProgressDto(ProjectId Id, Progress Progress) : ProgressDto<ProjectId>(Id, Progress);
