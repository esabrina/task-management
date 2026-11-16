using TaskManagement.Domain.Entities;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.UnitTests.Domain;

public sealed class TaskItemTests
{
    [Fact]
    public void Update_WithNewValues_ReplacesTaskDataAndSetsUpdatedAt()
    {
        var originalStatus = new DomainTaskStatus();
        var updatedStatus = new DomainTaskStatus();
        var task = new TaskItem(Guid.NewGuid(), "Original", "Description", originalStatus, DateTimeOffset.UtcNow);
        var dueDate = DateTimeOffset.UtcNow.AddDays(2);

        task.Update("Updated", "New description", updatedStatus, dueDate);

        Assert.Equal("Updated", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Same(updatedStatus, task.Status);
        Assert.Equal(updatedStatus.Id, task.StatusId);
        Assert.Equal(dueDate, task.DueDate);
        Assert.NotNull(task.UpdatedAt);
    }
}
