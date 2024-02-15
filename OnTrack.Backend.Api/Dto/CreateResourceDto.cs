using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class CreateResourceDto : IDto<Resource>
{
	public string Name { get; set; }
	public string? Description { get; set; }
	public int Quantity { get; set; }
	public string Unit { get; set; }

	public Resource ToDomainModel()
	{
		return new Resource
		{
			Name = Name,
			Description = Description,
			Quantity = Quantity,
			Unit = Unit
		};
	}
}
