using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Application.Mappings;

public interface IMapper<TEntity, TEntityId, TDto>
	where TEntity : IEntity<TEntityId>
	where TEntityId : IStronglyTypedId
	where TDto : IDto
{
	TEntity FromId(TEntityId id);

	void ToExistingDomainModel(TDto dto, TEntity entity);
	void ToExistingDto(TEntity entity, TDto dto);

	TEntityId ToId(TEntity entity);

	TEntity ToNewDomainModel(TDto dto);
	TDto ToNewDto(TEntity entity);
}
