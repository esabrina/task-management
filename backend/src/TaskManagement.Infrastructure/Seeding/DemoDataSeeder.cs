using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Identity;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Seeding;

public sealed class DemoDataSeeder(
    UserManager<ApplicationUser> userManager,
    TaskManagementDbContext dbContext)
{
    private readonly string DemoEmail = "user@demo.com";
    private readonly string DemoPass = "demo1234";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(DemoEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = DemoEmail,
                Email = DemoEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, DemoPass);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create demo user: {errors}");
            }
        }

        var existingTaskTitles = await dbContext.Task
            .Where(task => task.UserId == user.Id)
            .Select(task => task.Title)
            .ToListAsync(cancellationToken);

        var statuses = await dbContext.TaskStatus
            .ToDictionaryAsync(status => status.Id, cancellationToken);

        var tasks = new[]
        {
            CreateTask(
                user.Id,
                "Create backend project",
                "Set up the backend solution and project structure.",
                1,
                "2026-11-21T18:00:00+00:00",
                "2026-11-15T09:00:00+00:00",
                "2026-11-15T09:00:00+00:00",
                statuses),

            CreateTask(
                user.Id,
                "Implement user authentication",
                "Add JWT-based authentication and login validation.",
                3,
                "2026-11-20T18:00:00+00:00",
                "2026-11-15T10:00:00+00:00",
                "2026-11-15T10:00:00+00:00",
                statuses),

            CreateTask(
                user.Id,
                "Create task management API",
                "Implement the REST endpoints for creating, updating, and deleting tasks.",
                2,
                "2026-11-18T17:00:00+00:00",
                "2026-11-14T09:30:00+00:00",
                "2026-11-15T14:20:00+00:00",
                statuses),

            CreateTask(
                user.Id,
                "Write integration tests",
                "Add integration tests covering the main task management scenarios.",
                1,
                "2026-11-12T18:00:00+00:00",
                "2026-11-10T11:00:00+00:00",
                "2026-11-12T16:45:00+00:00",
                statuses),

            CreateTask(
                user.Id,
                "Update project documentation",
                "Review and update the API and setup documentation.",
                3,
                "2026-11-10T12:00:00+00:00",
                "2026-11-08T15:00:00+00:00",
                "2026-11-10T11:30:00+00:00",
                statuses)
        }
        .Where(task => !existingTaskTitles.Contains(task.Title))
        .ToArray();

        if (tasks.Length == 0)
            return;

        dbContext.Task.AddRange(tasks);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TaskItem CreateTask(
        Guid userId,
        string title,
        string description,
        int statusId,
        string dueDate,
        string createdAt,
        string updatedAt,
        IReadOnlyDictionary<int, Domain.Entities.TaskStatus> statuses)
    {
        if (!statuses.TryGetValue(statusId, out var status))
        {
            throw new InvalidOperationException(
                $"Task status '{statusId}' does not exist.");
        }

        return TaskItem.CreateSeeded(
            userId,
            title,
            description,
            status,
            DateTimeOffset.Parse(dueDate),
            DateTimeOffset.Parse(createdAt),
            DateTimeOffset.Parse(updatedAt));
    }
}