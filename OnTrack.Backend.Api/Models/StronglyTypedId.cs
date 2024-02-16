namespace OnTrack.Backend.Api.Models;

public abstract record class StronglyTypedId : IStronglyTypedId, IComparable<StronglyTypedId>, IEquatable<StronglyTypedId>
{
	public Guid Value { get; init; } = Guid.NewGuid();

	public int CompareTo(StronglyTypedId? other)
	{
		return Value.CompareTo(other?.Value);
	}

	public sealed override string ToString()
	{
		return Value.ToString();
	}
}
