namespace OnTrack.Backend.Api;

public readonly record struct Percentage(double Value)
{
	public static implicit operator Percentage(double value)
	{
		return new(value);
	}

	public static implicit operator double(Percentage percentage)
	{
		return percentage.Value;
	}
}
