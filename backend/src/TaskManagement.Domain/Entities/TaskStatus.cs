namespace TaskManagement.Domain.Entities;

public sealed class TaskStatus
{
    public TaskStatus()
    {
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<TaskItem> Tasks { get; private set; } = [];
}
