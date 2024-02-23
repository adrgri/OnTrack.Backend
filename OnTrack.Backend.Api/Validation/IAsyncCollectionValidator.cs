namespace OnTrack.Backend.Api.Validation;

public interface IAsyncCollectionValidator<in T, TResult> : IValidator<IEnumerable<T>, IAsyncEnumerable<TResult>>, IAsyncValidator<T, TResult>;
