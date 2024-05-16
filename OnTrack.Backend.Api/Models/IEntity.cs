using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Backend.Api.Models;

public interface IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
{
	[Key]
#if ApplicationGeneratedGuidIdsAllowed == false
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#endif
	TEntityId Id { get; set; }
}
