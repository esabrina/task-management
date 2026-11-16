using AutoMapper;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.UnitTests.Utils;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.UnitTests.Application;

public sealed class TaskServiceTests
{
    [Fact]
    [Trait("Category", "Task")]
    public async Task GetAllAsync_WhenRepositoryReturnsTasks_ReturnsMappedTasks()
    {
        var tasks = new[] { MockedData.Task(title: "First"), MockedData.Task(title: "Second") };
        var repository = new Mock<ITaskRepository>();
        var mapper = new Mock<IMapper>();
        repository.Setup(x => x.GetAllForUserAsync(MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);
        mapper.Setup(x => x.Map<TaskResponseDto>(It.IsAny<TaskItem>()))
            .Returns((TaskItem task) => new TaskResponseDto(task.Id, task.Title, task.Description,
                task.StatusId, task.Status!.Name, task.DueDate, task.CreatedAt, task.UpdatedAt));
        var service = new TaskService(repository.Object, Mock.Of<ITaskStatusRepository>(),
            Mock.Of<IUnitOfWork>(), mapper.Object);

        var result = await service.GetAllAsync(MockedData.UserId, CancellationToken.None);

        Assert.Equal(["First", "Second"], result.Select(task => task.Title));
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsMappedTask()
    {
        var task = MockedData.Task();
        var repository = new Mock<ITaskRepository>();
        var mapper = new Mock<IMapper>();
        repository.Setup(x => x.GetByIdForUserAsync(task.Id, MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        mapper.Setup(x => x.Map<TaskResponseDto>(task))
            .Returns(new TaskResponseDto(task.Id, task.Title, task.Description, task.StatusId,
                task.Status!.Name, task.DueDate, task.CreatedAt, task.UpdatedAt));
        var service = new TaskService(repository.Object, Mock.Of<ITaskStatusRepository>(),
            Mock.Of<IUnitOfWork>(), mapper.Object);

        var result = await service.GetByIdAsync(task.Id, MockedData.UserId, CancellationToken.None);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task GetAllTaskStatusesAsync_WhenStatusesExist_ReturnsMappedStatuses()
    {
        var statuses = new[] { MockedData.Status(1, "To-do"), MockedData.Status(2, "Doing") };
        var repository = new Mock<ITaskStatusRepository>();
        var mapper = new Mock<IMapper>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(statuses);
        mapper.Setup(x => x.Map<TaskStatusResponseDto>(It.IsAny<DomainTaskStatus>()))
            .Returns((DomainTaskStatus status) => new TaskStatusResponseDto(status.Id, status.Name));
        var service = new TaskService(Mock.Of<ITaskRepository>(), repository.Object,
            Mock.Of<IUnitOfWork>(), mapper.Object);

        var result = await service.GetAllTaskStatusesAsync(CancellationToken.None);

        Assert.Equal(["To-do", "Doing"], result.Select(status => status.Name));
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task UpdateAsync_WithValidRequest_UpdatesTaskAndSavesChanges()
    {
        var task = MockedData.Task();
        var status = MockedData.Status(2, "Doing");
        var repository = new Mock<ITaskRepository>();
        var statusRepository = new Mock<ITaskStatusRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        repository.Setup(x => x.GetByIdForUserAsync(task.Id, MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        statusRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(status);
        mapper.Setup(x => x.Map<TaskResponseDto>(task)).Returns(new TaskResponseDto(task.Id, "Updated",
            "Updated description", status.Id, status.Name, MockedData.DueDate.AddDays(1), task.CreatedAt, task.UpdatedAt));
        var service = new TaskService(repository.Object, statusRepository.Object, unitOfWork.Object, mapper.Object);

        var result = await service.UpdateAsync(task.Id, MockedData.UserId, MockedData.UpdateRequest(statusId: 2), CancellationToken.None);

        Assert.Equal("Updated", task.Title);
        Assert.Equal(status, task.Status);
        Assert.Equal("Updated", result.Title);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task CreateAsync_WhenTitleIsWhitespace_ThrowsRequestValidationException()
    {
        var service = new TaskService(Mock.Of<ITaskRepository>(), Mock.Of<ITaskStatusRepository>(),
            Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(
            MockedData.UserId, MockedData.CreateRequest(" "), CancellationToken.None));

        Assert.Equal(["Title field is required."], exception.Errors["title"]);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task CreateAsync_WithValidRequest_TrimsTitlePersistsTaskAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var dueDate = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var status = new DomainTaskStatus();
        var taskRepository = new Mock<ITaskRepository>();
        var statusRepository = new Mock<ITaskStatusRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mapper = new Mock<IMapper>();
        TaskItem? addedTask = null;
        taskRepository.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Callback<TaskItem, CancellationToken>((task, _) => addedTask = task)
            .Returns(Task.CompletedTask);
        mapper.Setup(x => x.Map<TaskResponseDto>(It.IsAny<TaskItem>()))
            .Returns((TaskItem task) => new TaskResponseDto(
                task.Id, task.Title, task.Description, task.StatusId, string.Empty,
                task.DueDate, task.CreatedAt, task.UpdatedAt));
        statusRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);
        var service = new TaskService(taskRepository.Object, statusRepository.Object, unitOfWork.Object, mapper.Object);

        var result = await service.CreateAsync(
            userId,
            new CreateTaskRequestDto
            {
                Title = "  New task  ",
                Description = "Details",
                StatusId = 1,
                DueDate = dueDate
            },
            CancellationToken.None);

        Assert.Equal("New task", result.Title);
        Assert.NotNull(addedTask);
        Assert.Equal(userId, addedTask.UserId);
        taskRepository.Verify(x => x.AddAsync(It.Is<TaskItem>(task =>
            task.UserId == userId && task.Title == "New task" && task.Description == "Details" && task.DueDate == dueDate), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task CreateAsync_WhenStatusDoesNotExist_ThrowsRequestValidationException()
    {
        var statusRepository = new Mock<ITaskStatusRepository>();
        statusRepository.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTaskStatus?)null);
        var service = new TaskService(
            Mock.Of<ITaskRepository>(), statusRepository.Object, Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(
            Guid.NewGuid(),
            new CreateTaskRequestDto { Title = "Task", StatusId = 99, DueDate = DateTimeOffset.UtcNow },
            CancellationToken.None));

        Assert.Equal(["Task status is invalid."], exception.Errors["statusId"]);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task UpdateAsync_WhenTaskIsNotOwned_ThrowsNotFoundException()
    {
        var repository = new Mock<ITaskRepository>();
        repository.Setup(x => x.GetByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        var service = new TaskService(
            repository.Object, Mock.Of<ITaskStatusRepository>(), Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(
            Guid.NewGuid(), Guid.NewGuid(),
            new UpdateTaskRequestDto { Title = "Updated", StatusId = 1, DueDate = DateTimeOffset.UtcNow },
            CancellationToken.None));

        Assert.Equal("Task not found.", exception.Message);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task DeleteAsync_WhenTaskExists_RemovesTaskAndSavesChanges()
    {
        var task = new TaskItem(Guid.NewGuid(), "Task", null, new DomainTaskStatus(), DateTimeOffset.UtcNow);
        var repository = new Mock<ITaskRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdForUserAsync(task.Id, task.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        var service = new TaskService(
            repository.Object, Mock.Of<ITaskStatusRepository>(), unitOfWork.Object, Mock.Of<IMapper>());

        await service.DeleteAsync(task.Id, task.UserId, CancellationToken.None);

        repository.Verify(x => x.Remove(task), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
