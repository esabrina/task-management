using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs;

namespace TaskManagement.IntegrationTests;

public abstract class AuthenticatedIntegrationTestBase(CustomWebApplicationFactory factory)
{
    protected CustomWebApplicationFactory Factory { get; } = factory;

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var credentials = new RegisterRequestDto
        {
            Email = $"user-{Guid.NewGuid():N}@example.com",
            Password = "password"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", credentials);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = credentials.Email,
            Password = credentials.Password
        });
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>()
            ?? throw new InvalidOperationException("The login response was empty.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        return client;
    }
}
