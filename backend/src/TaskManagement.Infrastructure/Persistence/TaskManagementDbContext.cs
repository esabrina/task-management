using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Identity;

namespace TaskManagement.Infrastructure.Persistence;

public sealed class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<TaskItem> Task => Set<TaskItem>();
    public DbSet<Domain.Entities.TaskStatus> TaskStatus => Set<Domain.Entities.TaskStatus>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .ToTable("AspNetUser")
            .Property(user => user.CreatedAt)
            .IsRequired();
        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ix_aspnetuser_email");

        builder.Entity<Domain.Entities.TaskStatus>(entity =>
        {
            entity.ToTable("TaskStatus");
            entity.HasKey(status => status.Id);
            entity.Property(status => status.Name).HasMaxLength(32).IsRequired();
            entity.HasData(
                new { Id = 1, Name = "To-do" },
                new { Id = 2, Name = "Doing" },
                new { Id = 3, Name = "Done" },
                new { Id = 4, Name = "Cancelled" });
        });

        builder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Title).HasMaxLength(100).IsRequired();
            entity.Property(task => task.Description).HasMaxLength(1000);
            entity.Property(task => task.DueDate).IsRequired();
            entity.Property(task => task.CreatedAt).IsRequired();
            entity.HasOne(task => task.Status)
                .WithMany(status => status.Tasks)
                .HasForeignKey(task => task.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(task => task.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(task => new { task.UserId, task.DueDate });
        });
    }
}
