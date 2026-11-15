using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Mapping;
using TaskManagement.Application.Services;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TaskMappingProfile).Assembly);
        services.AddScoped<AuthenticationService>();
        services.AddScoped<Interfaces.ITaskService, TaskService>();
        return services;
    }
}
