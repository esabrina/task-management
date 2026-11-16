using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagement.Infrastructure.Identity;

namespace TaskManagement.UnitTests.Infrastructure;

public sealed class IdentityTests
{
    [Fact]
     public async Task RegisterAsync_WhenUserManagerSucceeds_ReturnsSuccessfulResult()
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "password"))
            .ReturnsAsync(IdentityResult.Success);
        var service = new IdentityService(userManager.Object);

        var result = await service.RegisterAsync("user@example.com", "password", CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(result.EmailAlreadyExists);
        Assert.Empty(result.Errors);
        userManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(user =>
            user.Email == "user@example.com" && user.UserName == "user@example.com"), "password"), Times.Once);
    }

    [Fact]

    public async Task RegisterAsync_WhenEmailIsDuplicate_ReturnsDuplicateResult()
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "Email already exists."
            }));
        var service = new IdentityService(userManager.Object);

        var result = await service.RegisterAsync("user@example.com", "password", CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.EmailAlreadyExists);
        Assert.Equal(["Email already exists."], result.Errors);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WhenCredentialsAreValid_ReturnsUserId()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@example.com" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "password")).ReturnsAsync(true);
        var service = new IdentityService(userManager.Object);

        var result = await service.ValidateCredentialsAsync(user.Email, "password", CancellationToken.None);

        Assert.Equal(user.Id, result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WhenCredentialsAreInvalid_ReturnsNull()
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("user@example.com")).ReturnsAsync((ApplicationUser?)null);
        var service = new IdentityService(userManager.Object);

        var result = await service.ValidateCredentialsAsync("user@example.com", "password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WhenCancellationIsRequested_ThrowsCancellationException()
    {
        var service = new IdentityService(CreateUserManager().Object);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => service.RegisterAsync(
            "user@example.com", "password", cancellation.Token));
    }

    [Fact]
    public void Generate_WithConfiguredOptions_ReturnsTokenWithExpectedClaims()
    {
        var userId = Guid.NewGuid();
        var options = Options.Create(new JwtOptions
        {
            Key = "a-secret-key-that-is-at-least-32-characters-long",
            Issuer = "task-management",
            Audience = "task-management-client",
            ExpirationMinutes = 30
        });
        var generator = new JwtTokenGenerator(options);

        var generatedAt = DateTimeOffset.UtcNow;
        var result = generator.Generate(userId, "user@example.com");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Equal(userId.ToString(), token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("user@example.com", token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("task-management", token.Issuer);
        Assert.Equal("task-management-client", token.Audiences.Single());
        Assert.InRange(result.ExpiresAt, generatedAt.AddMinutes(30), generatedAt.AddMinutes(30).AddSeconds(2));
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
    }
}
