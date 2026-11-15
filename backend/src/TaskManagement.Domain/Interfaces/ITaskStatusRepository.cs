using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskStatusRepository
{
    Task<Entities.TaskStatus?> GetByIdAsync(int statusId, CancellationToken cancellationToken);
}
