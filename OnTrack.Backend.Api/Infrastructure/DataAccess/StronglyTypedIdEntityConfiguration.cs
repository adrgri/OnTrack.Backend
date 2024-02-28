using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

/*/ This class can not be abstract since EF Core must be able to create instances of it (that is if you don't create concrete child classes for every Entity) /*/
public class StronglyTypedIdEntityConfiguration<TEntityId, TEntity> : IStronglyTypedIdConverter<TEntityId>, IEntityTypeConfiguration<TEntity>
	where TEntityId : IStronglyTypedId, new()
	where TEntity : class, IEntity<TEntityId>
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		// TODO: Zrób benchmarki z różną wielkością tabel aby sprawdzić, jak się zachowa baza danych jeśli nagle zacznę generować klucze w aplikacji
		// podczas gdy wcześniej były generowane przez SQL Server z użyciem clustered index
		_ = builder.HasKey(entity => entity.Id)
			.IsClustered(false);

		// Configuration for converting the strongly typed Ids to and from the primitive value that will be stored in the database, in this case to and from a Guid
		_ = builder.Property(entity => entity.Id)
			.HasConversion(IStronglyTypedIdConverter<TEntityId>.IdConverter)
			// enable the strongly typed Id to be auto-generated when adding new entities to the database using the SaveChanges method (only works from EF Core 7 onwards)
			/*/ WARNING: IF YOU USE THIS METHOD, YOU MUST MAKE SURE THAT EITHER THE ID IS NEVER SET BY THE APPLICATION OR THE CLUSTERED PRIMARY INDEX IS DISABLED, /*/
			/*/ OTHERWISE YOU WILL REINTRODUCE A BUG FIXED IN THIS COMMIT /*/
			.ValueGeneratedOnAdd();
	}
}
