namespace OnTrack.Backend.Api.Dto;

public interface IDto<TDomainModel>
{
	TDomainModel ToDomainModel();
}
