
namespace TaskManagement.Application.DTOs;

public sealed record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    int StatusId,
    string Status,
    DateTimeOffset DueDate,
    DateTimeOffset DateCreated,
    DateTimeOffset? UpdatedAt);
