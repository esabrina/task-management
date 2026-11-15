
namespace TaskManagement.Domain.Interfaces;

public interface ITaskStatusRepository
{
    Task<Entities.TaskStatus?> GetByIdAsync(int statusId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Entities.TaskStatus>> GetAllAsync(CancellationToken cancellationToken);
}
