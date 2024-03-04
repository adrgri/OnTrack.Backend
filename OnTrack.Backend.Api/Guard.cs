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
			// TODO: Make the database generate the Id if the id is an empty guid,
			// it should not be null here
			if (entity.Id is null)
			{
				return;
			}

			if (entity.Id.Value != Guid.Empty)
			{
				throw new InvalidOperationException($"Application generated Guid Ids are not allowed! You must leave the Id empty by using {nameof(Guid)}.{nameof(Guid.Empty)} because the database will generate it for you.");
			}
		}
#endif
	}
}
