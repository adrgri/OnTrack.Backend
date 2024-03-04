using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api;

public static class Guard
{
	public static class Against
	{
#if ApplicationGeneratedGuidIdsAllowed == false
		public static void InsertingEntitiesWithApplicationGeneratedGuidIds<TEntity, TEntityId>(TEntity entity)
			where TEntity : IEntity<TEntityId>
			where TEntityId : IStronglyTypedId
		{
			if (entity.Id.Value != Guid.Empty)
			{
				throw new InvalidOperationException($"Application generated Guid Ids are not allowed! You must leave the Id empty by using {nameof(Guid)}.{nameof(Guid.Empty)} because the database will generate it for you.");
			}
		}
#endif
	}
}
