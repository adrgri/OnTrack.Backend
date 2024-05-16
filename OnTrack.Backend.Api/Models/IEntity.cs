using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Backend.Api.Models;

public interface IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
{
	[Key]
#if ApplicationGeneratedGuidIdsAllowed == false
	// TODO: This Id will potentially be generated either by the application or by the database, this also depends on the configuration of the IStronglyTypedId implementation,
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#endif
	TEntityId Id { get; set; }
}
