using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

public sealed class TaskRepository(TaskManagementDbContext dbContext) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Task
            .AsNoTracking()
            .Include(task => task.Status)
            .Where(task => task.UserId == userId)
            .OrderBy(task => task.DueDate)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Task
            .Include(task => task.Status)
            .FirstOrDefaultAsync(
                task => task.Id == taskId && task.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await dbContext.Task.AddAsync(task, cancellationToken);
    }

    public void Remove(TaskItem task)
    {
        dbContext.Task.Remove(task);
    }
}
