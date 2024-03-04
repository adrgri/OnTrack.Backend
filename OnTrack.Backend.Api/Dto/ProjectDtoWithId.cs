using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class ProjectDtoWithId : ProjectDto, IDtoWithId<ProjectId>
{
	public ProjectId Id { get; set; }

	public ProjectDtoWithId()
	{

	}

	public ProjectDtoWithId(Project project, IMapper<ProjectId, Project, ProjectDto> mapper)
	{
		mapper.ToExistingDto(project, this);

		Id = project.Id;
	}
}
