using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

public sealed class CreateTaskRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; init; }

    [Required]
    [Range(1, 4)]
    public int? StatusId { get; init; }

    [Required]
    public DateTimeOffset? DueDate { get; init; }
}