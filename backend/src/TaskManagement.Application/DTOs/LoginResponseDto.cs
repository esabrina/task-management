
namespace TaskManagement.Application.DTOs;

public sealed record LoginResponseDto(string AccessToken, DateTimeOffset ExpiresAt);

