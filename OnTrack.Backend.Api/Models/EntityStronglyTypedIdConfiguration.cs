using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OnTrack.Backend.Api.Models;

/*/ This class can not be abstract since EF Core must be able to create instances of it (if you don't create concrete child classes anywhere) /*/
public class EntityStronglyTypedIdConfiguration<TEntityId, TEntity> : IStronglyTypedIdConverter<TEntityId>, IEntityTypeConfiguration<TEntity>
	where TEntityId : IStronglyTypedId, new()
	where TEntity : class, IEntity<TEntityId>
{
	public static ValueConverter<TEntityId, Guid> IdConverter { get; } = new(
		// from Strong ID to Primitive value
		id => id.Value,
		// from Primitive value to Strong ID
		databasePrimaryKey => new TEntityId() { Value = databasePrimaryKey });

	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		_ = builder.HasKey(entity => entity.Id);

		// Configuration for converting the `id` property To that will be stored in the database and From the primitive value (e.g. GUID, int, string) back into a strongly typed Id:
		_ = builder.Property(entity => entity.Id)
			.HasConversion(IdConverter)
			// enable the primitive value to be auto-generated when adding (SaveChanges) new entities to the database (only works from EF Core 7 onwards)
			.ValueGeneratedOnAdd();
	}
}
