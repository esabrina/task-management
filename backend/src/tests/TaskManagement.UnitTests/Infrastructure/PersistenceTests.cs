using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.UnitTests.Utils;

namespace TaskManagement.UnitTests.Infrastructure;

public sealed class PersistenceTests
{
    [Fact]

    public async Task TaskRepository_GetAllForUserAsync_ReturnsTasksOrderedByDueDate()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var status = context.TaskStatus.Single(item => item.Id == 1);
        var first = new TaskItem(MockedData.UserId, "First", "Description", status, MockedData.DueDate);
        var second = new TaskItem(MockedData.UserId, "Second", "Description", status, MockedData.DueDate.AddDays(-1));
        context.Task.AddRange(first, second);
        await context.SaveChangesAsync();

        var result = await repository.GetAllForUserAsync(MockedData.UserId, CancellationToken.None);

        Assert.Equal(["Second", "First"], result.Select(task => task.Title));
        Assert.All(result, task => Assert.NotNull(task.Status));
    }

    [Fact]

    public async Task TaskRepository_GetByIdForUserAsync_ReturnsOnlyMatchingUserTask()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var task = new TaskItem(MockedData.UserId, "Task", "Description",
            context.TaskStatus.Single(item => item.Id == 1), MockedData.DueDate);
        context.Task.Add(task);
        await context.SaveChangesAsync();

        var found = await repository.GetByIdForUserAsync(task.Id, MockedData.UserId, CancellationToken.None);
        var notFound = await repository.GetByIdForUserAsync(task.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Same(task, found);
        Assert.Null(notFound);
    }

    [Fact]

    public async Task TaskRepository_AddAndRemoveAsync_ChangesStoredTasks()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var task = new TaskItem(MockedData.UserId, "Task", "Description",
            context.TaskStatus.Single(item => item.Id == 1), MockedData.DueDate);

        await repository.AddAsync(task, CancellationToken.None);
        await context.SaveChangesAsync();
        repository.Remove(task);
        await context.SaveChangesAsync();

        Assert.Empty(context.Task);
    }

    [Fact]

    public async Task TaskStatusRepository_GetMethods_ReturnSeededStatuses()
    {
        await using var context = CreateContext();
        var repository = new TaskStatusRepository(context);

        var status = await repository.GetByIdAsync(2, CancellationToken.None);
        var statuses = await repository.GetAllAsync(CancellationToken.None);

        Assert.Equal("Doing", status!.Name);
        Assert.Equal([1, 2, 3, 4], statuses.Select(item => item.Id));
    }

    [Fact]

    public async Task UnitOfWork_SaveChangesAsync_PersistsChanges()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        context.Task.Add(new TaskItem(MockedData.UserId, "Task", "Description",
            context.TaskStatus.Single(item => item.Id == 1), MockedData.DueDate));

        var changes = await unitOfWork.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(1, changes);
        Assert.Single(context.Task);
    }

    private static TaskManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TaskManagementDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
