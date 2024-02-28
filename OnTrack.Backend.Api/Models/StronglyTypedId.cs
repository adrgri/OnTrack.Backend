namespace OnTrack.Backend.Api.Models;

public abstract record class StronglyTypedId : IStronglyTypedId, IComparable<StronglyTypedId>, IEquatable<StronglyTypedId>
{
	// Let the database generate the value for this property in case it is needed, the caller can still provide a value if they want to by using the object initializer syntax
	public Guid Value { get; init; } = Guid.Empty;

	public int CompareTo(StronglyTypedId? other)
	{
		return Value.CompareTo(other?.Value);
	}

	public sealed override string ToString()
	{
		return Value.ToString();
	}
}
