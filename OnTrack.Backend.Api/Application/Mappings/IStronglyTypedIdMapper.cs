using OnTrack.Backend.Api.Dto;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Application.Mappings;

public interface IStronglyTypedIdMapper<TEntityId, TEntity, TDto> : IMapper<TEntityId, TEntity, TDto>
	where TEntityId : IStronglyTypedId
	where TEntity : IEntity<TEntityId>, new()
	where TDto : IDto;
