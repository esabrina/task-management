using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;

namespace TaskManagement.UnitTests.Api;

public sealed class AuthControllerTests
{
    [Fact]
    [Trait("Category", "Auth")]
    public async Task Register_WhenAuthenticationSucceeds_ReturnsCreatedResponse()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityRegistrationResult(true, false, []));
        var controller = new AuthController(new AuthenticationService(identity.Object, Mock.Of<IJwtTokenGenerator>()));
        var request = new RegisterRequestDto { Email = "user@example.com", Password = "password" };

        var result = await controller.Register(request, CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.IsType<RegisterResponseDto>(response.Value);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task Login_WhenAuthenticationSucceeds_ReturnsOkResponse()
    {
        var userId = Guid.NewGuid();
        var identity = new Mock<IIdentityService>();
        var tokenGenerator = new Mock<IJwtTokenGenerator>();
        identity.Setup(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        tokenGenerator.Setup(x => x.Generate(userId, It.IsAny<string>()))
            .Returns(new GeneratedJwt("access-token", DateTimeOffset.UtcNow.AddMinutes(30)));
        var controller = new AuthController(new AuthenticationService(identity.Object, tokenGenerator.Object));
        var request = new LoginRequestDto { Email = "user@example.com", Password = "password" };

        var result = await controller.Login(request, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        var value = Assert.IsType<LoginResponseDto>(response.Value);
        Assert.Equal("access-token", value.AccessToken);
    }
}
