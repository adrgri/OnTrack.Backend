namespace OnTrack.Backend.Api.Models;

public record class Entity<TEntityId> : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId, new()
{
	public TEntityId Id { get; set; }
}
