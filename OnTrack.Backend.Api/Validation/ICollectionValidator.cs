namespace OnTrack.Backend.Api.Validation;

public interface ICollectionValidator<in T, out TResult> : IValidator<IEnumerable<T>, IEnumerable<TResult>>, IValidator<T, TResult>;
