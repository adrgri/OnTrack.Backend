using OnTrack.Backend.Api.Application.Mappings;
using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Dto;

public sealed record class TaskDtoWithId : TaskDto, IDtoWithId<TaskId>
{
	public TaskId Id { get; set; }

	public TaskDtoWithId()
	{
		
	}

	public TaskDtoWithId(Task task, IMapper<TaskId, Task, TaskDto> mapper)
	{
		mapper.ToExistingDto(task, this);

		Id = task.Id;
	}
}
