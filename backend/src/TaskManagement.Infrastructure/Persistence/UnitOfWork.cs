using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

public sealed class UnitOfWork(TaskManagementDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
