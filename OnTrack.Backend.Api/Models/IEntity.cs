using System.ComponentModel.DataAnnotations;

namespace OnTrack.Backend.Api.Models;

public interface IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
{
	[Key]
	TEntityId Id { get; set; }
}
