namespace TaskManagement.Domain.Entities;

public sealed class TaskItem
{
    private TaskItem()
    {
        // needed for PersistenceTests
    }

    public TaskItem(
        Guid userId,
        string title,
        string? description,
        TaskStatus status,
        DateTimeOffset dueDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Description = description;
        Status = status;
        StatusId = status.Id;
        DueDate = dueDate;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int StatusId { get; private set; }
    public TaskStatus? Status { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string title, string? description, TaskStatus status, DateTimeOffset dueDate)
    {
        Title = title;
        Description = description;
        Status = status;
        StatusId = status.Id;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static TaskItem CreateSeeded(
        Guid userId,
        string title,
        string? description,
        TaskStatus status,
        DateTimeOffset dueDate,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        return new TaskItem(userId, title, description, status, dueDate)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
