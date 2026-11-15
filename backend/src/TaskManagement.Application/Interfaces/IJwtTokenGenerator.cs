namespace TaskManagement.Application.Interfaces;

public sealed record GeneratedJwt(string AccessToken, DateTimeOffset ExpiresAt);

public interface IJwtTokenGenerator
{
    GeneratedJwt Generate(Guid userId, string email);
}
