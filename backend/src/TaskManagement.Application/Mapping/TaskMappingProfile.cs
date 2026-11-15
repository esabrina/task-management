using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mapping;

public sealed class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskResponseDto>()
            .ForCtorParam(
                nameof(TaskResponseDto.Status),
                options => options.MapFrom(source =>
                    source.Status == null ? string.Empty : source.Status.Name))
            .ForCtorParam(
                nameof(TaskResponseDto.DateCreated),
                options => options.MapFrom(source => source.CreatedAt));

        CreateMap<Domain.Entities.TaskStatus, TaskStatusResponseDto>();
    }
}
