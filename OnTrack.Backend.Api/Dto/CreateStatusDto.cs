using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public class CreateStatusDto : IDto<Status>
{
	public string Name { get; set; }

	public Status ToDomainModel()
	{
		return new Status()
		{
			Name = Name
		};
	}
}
