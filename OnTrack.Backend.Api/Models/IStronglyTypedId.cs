namespace OnTrack.Backend.Api.Models;

public interface IStronglyTypedId
{
	Guid Value { get; init; }

	string ToString();
}
