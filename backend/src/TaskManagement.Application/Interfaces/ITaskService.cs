using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponseDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<TaskResponseDto> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponseDto> CreateAsync(Guid userId, CreateTaskRequestDto request, CancellationToken cancellationToken);
    Task<TaskResponseDto> UpdateAsync(Guid taskId, Guid userId, UpdateTaskRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskStatusResponseDto>> GetAllTaskStatusesAsync(CancellationToken cancellationToken);
}
