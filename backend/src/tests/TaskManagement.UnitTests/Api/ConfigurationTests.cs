using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskManagement.Application;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mapping;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure;
using TaskManagement.UnitTests.Utils;

namespace TaskManagement.UnitTests.Api;

public sealed class ConfigurationTests
{
    [Fact]
    public void AddApplication_RegistersApplicationServicesAndMappings()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Mock.Of<IIdentityService>());
        services.AddSingleton(Mock.Of<IJwtTokenGenerator>());
        services.AddSingleton(Mock.Of<ITaskRepository>());
        services.AddSingleton(Mock.Of<ITaskStatusRepository>());
        services.AddSingleton(Mock.Of<IUnitOfWork>());
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<TaskManagement.Application.Services.AuthenticationService>());
        Assert.NotNull(provider.GetService<ITaskService>());
        Assert.NotNull(provider.GetService<IMapper>());
    }

    [Fact]
    public void TaskMappingProfile_MapsTaskStatusAndTaskFields()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<TaskMappingProfile>());
        var mapper = configuration.CreateMapper();
        var task = MockedData.Task();

        var result = mapper.Map<TaskResponseDto>(task);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
        Assert.Equal(task.Status!.Name, result.Status);
        Assert.Equal(task.CreatedAt, result.DateCreated);
    }

    [Fact]
    public void AddInfrastructure_WithConnectionString_RegistersInfrastructureServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=tasks"
            })
            .Build();

        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ITaskRepository>());
        Assert.NotNull(provider.GetService<ITaskStatusRepository>());
        Assert.NotNull(provider.GetService<IIdentityService>());
        Assert.NotNull(provider.GetService<IJwtTokenGenerator>());
    }

    [Fact]
    public void AddInfrastructure_WithoutConnectionString_ThrowsConfigurationException()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddInfrastructure(new ConfigurationBuilder().Build()));
    }
}
