using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.Models;

public interface IEntity<out TEntityId>
	where TEntityId : IStronglyTypedId
{
	[Key]
	TEntityId Id { get; }
}
