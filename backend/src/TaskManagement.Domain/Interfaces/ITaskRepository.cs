using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);
    void Remove(TaskItem task);
}
