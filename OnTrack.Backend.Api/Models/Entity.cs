using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Backend.Api.Models;

public record class Entity<TEntityId> : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId, new()
{
	[Key]
#if ApplicationGeneratedGuidIdsAllowed == false
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#endif
	public TEntityId Id { get; set; }
}
