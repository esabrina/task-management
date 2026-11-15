using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Application.Services;

public sealed class AuthenticationService(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request.Email, request.Password, cancellationToken);
        if (result.Succeeded)
        {
            return new RegisterResponseDto("User registered successfully.");
        }

        if (result.EmailAlreadyExists)
        {
            throw new ConflictException("Email is already registered.");
        }

        throw new RequestValidationException(new Dictionary<string, string[]>
        {
            ["registration"] = result.Errors
        });
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var userId = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (userId is null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var token = jwtTokenGenerator.Generate(userId.Value, request.Email);
        return new LoginResponseDto(token.AccessToken, token.ExpiresAt);
    }
}
