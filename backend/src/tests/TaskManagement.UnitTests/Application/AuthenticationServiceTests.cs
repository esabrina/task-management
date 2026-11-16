using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;

namespace TaskManagement.UnitTests.Application;

public sealed class AuthenticationServiceTests
{
    [Fact]
    [Trait("Category", "Auth")]
    public async Task RegisterAsync_WhenRegistrationSucceeds_ReturnsSuccessMessage()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.RegisterAsync("user@example.com", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRegistrationResult(true, false, []));
        var service = new AuthenticationService(identity.Object, Mock.Of<IJwtTokenGenerator>());

        var result = await service.RegisterAsync(
            new RegisterRequestDto { Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        Assert.Equal("User registered successfully.", result.Message);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRegistrationResult(false, true, []));
        var service = new AuthenticationService(identity.Object, Mock.Of<IJwtTokenGenerator>());

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(
            new RegisterRequestDto { Email = "user@example.com", Password = "password" },
            CancellationToken.None));

        Assert.Equal("Email is already registered.", exception.Message);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task RegisterAsync_WhenIdentityValidationFails_ThrowsRequestValidationExceptionWithErrors()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRegistrationResult(false, false, ["Password is too weak."]));
        var service = new AuthenticationService(identity.Object, Mock.Of<IJwtTokenGenerator>());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.RegisterAsync(
            new RegisterRequestDto { Email = "user@example.com", Password = "password" },
            CancellationToken.None));

        Assert.Equal(["Password is too weak."], exception.Errors["registration"]);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task LoginAsync_WhenCredentialsAreInvalid_ThrowsUnauthorizedException()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        var service = new AuthenticationService(identity.Object, Mock.Of<IJwtTokenGenerator>());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(
            new LoginRequestDto { Email = "user@example.com", Password = "wrong" },
            CancellationToken.None));

        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsGeneratedToken()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var identity = new Mock<IIdentityService>();
        var tokenGenerator = new Mock<IJwtTokenGenerator>();
        identity.Setup(x => x.ValidateCredentialsAsync("user@example.com", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        tokenGenerator.Setup(x => x.Generate(userId, "user@example.com"))
            .Returns(new GeneratedJwt("access-token", expiresAt));
        var service = new AuthenticationService(identity.Object, tokenGenerator.Object);

        var result = await service.LoginAsync(
            new LoginRequestDto { Email = "user@example.com", Password = "password" },
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
    }
}
