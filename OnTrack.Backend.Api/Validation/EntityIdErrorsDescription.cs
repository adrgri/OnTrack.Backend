using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Validation;

public sealed record class EntityIdErrorsDescription<TEntityId>(TEntityId EntityId, List<string> ErrorsDescription)
	where TEntityId : IStronglyTypedId;
