namespace OnTrack.Backend.Api.Validation;

public interface IAsyncValidator<in T, TResult> : IValidator<T, Task<TResult>>;
