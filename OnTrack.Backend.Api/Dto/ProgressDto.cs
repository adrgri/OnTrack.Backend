using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public record class ProgressDto<TEntityId>(TEntityId Id, Progress Progress) : IDtoWithId<TEntityId>
	where TEntityId : IStronglyTypedId
{
	public TEntityId Id { get; set; } = Id;
	public Progress Progress { get; set; } = Progress;
}
