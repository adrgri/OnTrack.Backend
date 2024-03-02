using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Application.Mappings;

public abstract class StronglyTypedIdMapper<TEntityId, TEntity, TDto> : IMapper<TEntityId, TEntity, TDto>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>, new()
	where TDto : IDto
{
	public TEntity FromId(TEntityId id)
	{
		return new TEntity
		{
			Id = id
		};
	}

	public abstract void ToExistingDomainModel(TDto dto, TEntity entity);

	public abstract void ToExistingDto(TEntity entity, TDto dto);

	public TEntityId ToId(TEntity entity)
	{
		return entity.Id;
	}

	public abstract TEntity ToNewDomainModel(TDto dto);

	public abstract TDto ToNewDto(TEntity entity);
}
