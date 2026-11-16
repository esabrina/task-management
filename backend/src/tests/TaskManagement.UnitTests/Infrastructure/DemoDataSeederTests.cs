using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Identity;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Seeding;

namespace TaskManagement.UnitTests.Infrastructure;

public sealed class DemoDataSeederTests
{
    [Fact]
    [Trait("Category", "Seeding")]
    public async Task SeedAsync_WhenCalledMoreThanOnce_CreatesDemoUserAndTasksOnlyOnce()
    {
        await using var context = CreateContext();
        ApplicationUser? demoUser = null;
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("user@demo.com"))
            .Returns(() => Task.FromResult(demoUser));
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "demo1234"))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                demoUser = user;
                context.Users.Add(user);
            })
            .ReturnsAsync(IdentityResult.Success);
        var seeder = new DemoDataSeeder(userManager.Object, context);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.NotNull(demoUser);
        Assert.Equal("user@demo.com", demoUser.Email);
        Assert.Equal(5, await context.Task.CountAsync());
        userManager.Verify(
            manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "demo1234"),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Seeding")]
    public async Task SeedAsync_WhenSomeDemoTasksAlreadyExist_AddsOnlyMissingTasks()
    {
        await using var context = CreateContext();
        var existingUser = CreateUser();
        context.Users.Add(existingUser);
        var existingTask = new TaskItem(
            existingUser.Id,
            "Create backend project",
            "Existing task",
            context.TaskStatus.Single(status => status.Id == 1),
            DateTimeOffset.UtcNow);
        context.Task.Add(existingTask);
        await context.SaveChangesAsync();

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("user@demo.com"))
            .ReturnsAsync(existingUser);
        var seeder = new DemoDataSeeder(userManager.Object, context);

        await seeder.SeedAsync();

        var tasks = await context.Task.OrderBy(task => task.Title).ToListAsync();
        Assert.Equal(5, tasks.Count);
        Assert.Contains(tasks, task => task.Id == existingTask.Id && task.Description == "Existing task");
        userManager.Verify(
            manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Seeding")]
    public async Task SeedAsync_WhenDemoUserCreationFails_ThrowsWithIdentityErrors()
    {
        await using var context = CreateContext();
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("user@demo.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "demo1234"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation rejected." }));
        var seeder = new DemoDataSeeder(userManager.Object, context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());

        Assert.Equal("Could not create demo user: Creation rejected.", exception.Message);
        Assert.Empty(context.Task);
    }

    [Fact]
    [Trait("Category", "Seeding")]
    public async Task SeedAsync_WhenRequiredTaskStatusIsMissing_ThrowsAndDoesNotSaveTasks()
    {
        await using var context = CreateContext();
        var missingStatus = await context.TaskStatus.SingleAsync(status => status.Id == 3);
        context.TaskStatus.Remove(missingStatus);
        await context.SaveChangesAsync();

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("user@demo.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "demo1234"))
            .ReturnsAsync(IdentityResult.Success);
        var seeder = new DemoDataSeeder(userManager.Object, context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync());

        Assert.Equal("Task status '3' does not exist.", exception.Message);
        Assert.Empty(context.Task);
    }

    private static TaskManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TaskManagementDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@demo.com",
            UserName = "user@demo.com"
        };
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }
}

