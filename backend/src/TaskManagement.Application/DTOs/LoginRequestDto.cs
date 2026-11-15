using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

public sealed class LoginRequestDto
{
    [Required(ErrorMessage = "Invalid email or password")]
    [EmailAddress(ErrorMessage = "Invalid email or password")]
    [StringLength(256, ErrorMessage = "Invalid email or password")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Invalid email or password")]
    public string Password { get; init; } = string.Empty;
}