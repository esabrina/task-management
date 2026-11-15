using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

public sealed class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
}
