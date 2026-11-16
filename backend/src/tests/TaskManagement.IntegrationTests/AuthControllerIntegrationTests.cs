using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.DTOs;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.IntegrationTests;

public sealed class AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Register_ReturnsCreated_AndPersistsUser()
    {
        var request = NewRegisterRequest();

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        Assert.Equal("User registered successfully.", result?.Message);

        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
        Assert.True(await database.Users.AnyAsync(user => user.Email == request.Email));
    }

    [Fact]
    public async Task Login_WithRegisteredCredentials_ReturnsToken()
    {
        var request = NewRegisterRequest();
        await RegisterAsync(request);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = $"missing-{Guid.NewGuid():N}@example.com",
            Password = "password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        var request = NewRegisterRequest();
        await RegisterAsync(request);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ReturnsBadRequest()
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "not-an-email",
            Password = "short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task RegisterAsync(RegisterRequestDto request)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static RegisterRequestDto NewRegisterRequest() => new()
    {
        Email = $"user-{Guid.NewGuid():N}@example.com",
        Password = "password"
    };
}
