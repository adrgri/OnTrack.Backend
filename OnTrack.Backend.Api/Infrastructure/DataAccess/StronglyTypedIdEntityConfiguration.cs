using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Infrastructure.DataAccess;

/*/ This class can not be abstract since EF Core must be able to create instances of it (if you don't create concrete child classes anywhere) /*/
public class StronglyTypedIdEntityConfiguration<TEntityId, TEntity> : IStronglyTypedIdConverter<TEntityId>, IEntityTypeConfiguration<TEntity>
	where TEntityId : IStronglyTypedId, new()
	where TEntity : class, IEntity<TEntityId>
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		_ = builder.HasKey(entity => entity.Id);

		// Configuration for converting the `id` property To that will be stored in the database and From the primitive value (e.g. GUID, int, string) back into a strongly typed Id:
		_ = builder.Property(entity => entity.Id)
			.HasConversion(IStronglyTypedIdConverter<TEntityId>.IdConverter)
			// enable the primitive value to be auto-generated when adding (SaveChanges) new entities to the database (only works from EF Core 7 onwards)
			.ValueGeneratedOnAdd();
	}
}
