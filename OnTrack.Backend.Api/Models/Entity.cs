namespace OnTrack.Backend.Api.Models;

public record class Entity<TEntityId> : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId, new()
{
	// TODO: Czy ograniczenie new() jest potrzebne jeśli nie będzie zezwolenia na tworzenie identyfikatorów w aplikacji?
	// Być może EF Core będzie miał co do tego jakieś zastrzeżenia, np przy wyciąganiu danych z bazy
	public TEntityId Id { get; set; } //= new();
}
