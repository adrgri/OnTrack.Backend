using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class TaskProgressDto(TaskId Id, Progress Progress) : ProgressDto<TaskId>(Id, Progress);
