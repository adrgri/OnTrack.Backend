namespace OnTrack.Backend.Api.Validation;

public interface IValidator<in T, out TResult>
{
	TResult Validate(T item, CancellationToken cancellationToken);
	TResult Validate(T item)
	{
		return Validate(item, CancellationToken.None);
	}
}
