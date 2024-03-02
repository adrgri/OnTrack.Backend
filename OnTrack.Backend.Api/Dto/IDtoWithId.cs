using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public interface IDtoWithId<TEntityId> : IDto
	where TEntityId : IStronglyTypedId
{
	TEntityId Id { get; set; }
}
