using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Controllers;

namespace TaskManagement.UnitTests.Api;

public sealed class SwaggerConfigTests
{
    [Fact]
    [Trait("Category", "Swagger")]
    public void AddSwaggerConfig_GeneratesDocumentWithBearerAuthenticationForProtectedRoutes()
    {
        var document = CreateDocument();

        Assert.Equal("Task Management API", document.Info.Title);
        Assert.Equal("v1", document.Info.Version);

        var bearerScheme = document.Components.SecuritySchemes["Bearer"];
        Assert.Equal(SecuritySchemeType.Http, bearerScheme.Type);
        Assert.Equal("bearer", bearerScheme.Scheme);
        Assert.Equal("JWT", bearerScheme.BearerFormat);
        Assert.Equal("Authorization", bearerScheme.Name);
        Assert.Equal(ParameterLocation.Header, bearerScheme.In);

        var protectedOperation = document.Paths["/api/tasks"].Operations[OperationType.Get];
        Assert.Contains(protectedOperation.Security, requirement =>
            requirement.Keys.Any(scheme => scheme.Reference?.Id == "Bearer"));
    }

    [Fact]
    [Trait("Category", "Swagger")]
    public void AddSwaggerConfig_DoesNotRequireAuthenticationForAnonymousRoutes()
    {
        var document = CreateDocument();

        var registerOperation = document.Paths["/api/auth/register"].Operations[OperationType.Post];
        var loginOperation = document.Paths["/api/auth/login"].Operations[OperationType.Post];
        Assert.Empty(registerOperation.Security);
        Assert.Empty(loginOperation.Security);
    }

    private static OpenApiDocument CreateDocument()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateWebHostEnvironment());
        services
            .AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly)
            .AddControllersAsServices();
        services.AddSwaggerConfig();
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
    }

    private static IWebHostEnvironment CreateWebHostEnvironment()
    {
        return new TestWebHostEnvironment
        {
            ApplicationName = typeof(TasksController).Assembly.GetName().Name!,
            EnvironmentName = "Development",
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = AppContext.BaseDirectory,
            ContentRootFileProvider = new NullFileProvider(),
            WebRootFileProvider = new NullFileProvider()
        };
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
