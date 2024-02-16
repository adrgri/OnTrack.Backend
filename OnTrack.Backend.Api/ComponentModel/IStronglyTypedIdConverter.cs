using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.ComponentModel;

public interface IStronglyTypedIdConverter<TStronglyTypedId>
	where TStronglyTypedId : IStronglyTypedId, new()
{
	public static ValueConverter<TStronglyTypedId, Guid> IdConverter { get; } = new(
		 id => id.Value,
		  databasePrimaryKey => new TStronglyTypedId() { Value = databasePrimaryKey });
}
