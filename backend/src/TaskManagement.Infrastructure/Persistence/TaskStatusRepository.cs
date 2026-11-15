using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

public sealed class TaskStatusRepository(TaskManagementDbContext dbContext) : ITaskStatusRepository
{
    public Task<Domain.Entities.TaskStatus?> GetByIdAsync(int statusId, CancellationToken cancellationToken)
    {
        return dbContext.TaskStatus.FirstOrDefaultAsync(status => status.Id == statusId, cancellationToken);
    }
    public async Task<IReadOnlyList<Domain.Entities.TaskStatus>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TaskStatus
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }
}
