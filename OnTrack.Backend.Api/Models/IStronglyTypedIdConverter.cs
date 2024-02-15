using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OnTrack.Backend.Api.Models;

public interface IStronglyTypedIdConverter<TStronglyTypedId>
	where TStronglyTypedId : IStronglyTypedId
{
	static abstract ValueConverter<TStronglyTypedId, Guid> IdConverter { get; }
}
