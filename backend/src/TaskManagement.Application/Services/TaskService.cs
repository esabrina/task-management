using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    ITaskStatusRepository taskStatusRepository,
    IUnitOfWork unitOfWork) : ITaskService
{
    public async Task<IReadOnlyList<TaskResponseDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetAllForUserAsync(userId, cancellationToken);
        return tasks.Select(Map).ToArray();
    }

    public async Task<TaskResponseDto> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        return Map(task);
    }

    public async Task<TaskResponseDto> CreateAsync(
        Guid userId,
        CreateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidateTitle(request.Title);
        var status = await GetStatusAsync(request.StatusId!.Value, cancellationToken);

        var task = new TaskItem(
            userId,
            request.Title.Trim(),
            request.Description,
            status,
            request.DueDate!.Value.ToUniversalTime());

        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        //TODO: Add automapper to map entities to DTOs
        return Map(task);
    }

    public async Task<TaskResponseDto> UpdateAsync(
        Guid taskId,
        Guid userId,
        UpdateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        ValidateTitle(request.Title);
        var status = await GetStatusAsync(request.StatusId!.Value, cancellationToken);

        task.Update(request.Title.Trim(), request.Description, status, request.DueDate!.Value.ToUniversalTime());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        //TODO: Add automapper to map entities to DTOs
        return Map(task);
    }

    public async Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        taskRepository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskItem> GetOwnedTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        return await taskRepository.GetByIdForUserAsync(taskId, userId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
    }

    private async Task<Domain.Entities.TaskStatus> GetStatusAsync(int statusId, CancellationToken cancellationToken)
    {
        var status = await taskStatusRepository.GetByIdAsync(statusId, cancellationToken);
        if (status is null)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["statusId"] = ["The selected task status is invalid."]
            });
        }

        return status;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["title"] = ["The title field is required."]
            });
        }
    }

    private static TaskResponseDto Map(TaskItem task)
    {
        return new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.StatusId,
            task.Status?.Name ?? string.Empty,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
