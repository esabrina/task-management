using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.UnitTests.Utils;

internal static class MockedData
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly DateTimeOffset DueDate = new(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);

    public static DomainTaskStatus Status(int id = 1, string name = "To-do")
    {
        var status = new DomainTaskStatus();
        typeof(DomainTaskStatus).GetProperty(nameof(DomainTaskStatus.Id))!.SetValue(status, id);
        typeof(DomainTaskStatus).GetProperty(nameof(DomainTaskStatus.Name))!.SetValue(status, name);
        return status;
    }

    public static TaskItem Task(Guid? userId = null, string title = "Task", int statusId = 1)
    {
        return new TaskItem(userId ?? UserId, title, "Description", Status(statusId), DueDate);
    }

    public static CreateTaskRequestDto CreateRequest(string title = "Task", int statusId = 1)
    {
        return new CreateTaskRequestDto
        {
            Title = title,
            Description = "Description",
            StatusId = statusId,
            DueDate = DueDate
        };
    }

    public static UpdateTaskRequestDto UpdateRequest(string title = "Updated", int statusId = 1)
    {
        return new UpdateTaskRequestDto
        {
            Title = title,
            Description = "Updated description",
            StatusId = statusId,
            DueDate = DueDate.AddDays(1)
        };
    }
}
