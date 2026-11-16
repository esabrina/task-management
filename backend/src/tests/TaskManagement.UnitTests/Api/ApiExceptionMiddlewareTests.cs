using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Middleware;
using TaskManagement.Application.Exceptions;

namespace TaskManagement.UnitTests.Api;

public sealed class ApiExceptionMiddlewareTests
{
    [Theory]

    [InlineData(typeof(NotFoundException), 404, "not found")]
    [InlineData(typeof(ConflictException), 409, "conflict")]
    [InlineData(typeof(UnauthorizedException), 401, "unauthorized")]
    public async Task InvokeAsync_WhenKnownExceptionIsThrown_WritesExpectedProblem(
        Type exceptionType, int expectedStatusCode, string detail)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, detail)!;
        var middleware = new ApiExceptionMiddleware(_ => throw exception, Mock.Of<ILogger<ApiExceptionMiddleware>>());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.Equal(detail, response.RootElement.GetProperty("detail").GetString());
    }

    [Fact]

    public async Task InvokeAsync_WhenValidationExceptionIsThrown_WritesValidationProblem()
    {
        var exception = new RequestValidationException(new Dictionary<string, string[]>
        {
            ["title"] = ["Title is required."]
        });
        var middleware = new ApiExceptionMiddleware(_ => throw exception, Mock.Of<ILogger<ApiExceptionMiddleware>>());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.Equal("Title is required.", response.RootElement.GetProperty("errors").GetProperty("title")[0].GetString());
    }

    [Fact]

    public async Task InvokeAsync_WhenUnexpectedExceptionIsThrown_WritesInternalServerError()
    {
        var middleware = new ApiExceptionMiddleware(_ => throw new InvalidOperationException("failure"),
            Mock.Of<ILogger<ApiExceptionMiddleware>>());
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.Equal("An unexpected error occurred.", response.RootElement.GetProperty("detail").GetString());
    }

    [Fact]

    public async Task InvokeAsync_WhenRequestIsCanceled_CompletesWithoutResponse()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var middleware = new ApiExceptionMiddleware(_ => throw new OperationCanceledException(),
            Mock.Of<ILogger<ApiExceptionMiddleware>>());
        var context = CreateContext();
        context.RequestAborted = cancellation.Token;

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
