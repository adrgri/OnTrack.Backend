using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Application.Mappings;

// TODO: Niech mapper mapuje tylko w jedną stronę a nie w dwie, uogólnij ten interfejs trochę bardziej, zawsze można zaimplementować ten interfejs dwukrotnie zamieniając kolejność źródła i celu
public interface IMapper<TEntityId, TEntity, TDto>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>
	where TDto : IDto
{
	TEntity FromId(TEntityId id);

	void ToExistingDomainModel(TDto dto, TEntity entity);
	void ToExistingDto(TEntity entity, TDto dto);

	TEntityId ToId(TEntity entity);

	TEntity ToNewDomainModel(TDto dto);
	TDto ToNewDto(TEntity entity);
}
