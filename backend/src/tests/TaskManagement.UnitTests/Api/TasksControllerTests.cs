using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.UnitTests.Utils;

namespace TaskManagement.UnitTests.Api;

public sealed class TasksControllerTests
{
    [Fact]
    [Trait("Category", "Task")]
    public async Task GetAll_WhenUserIsAuthenticated_ReturnsTasksForAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskResponseDto>
        {
            new(Guid.NewGuid(), "Task", null, 1, "Open", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null)
        };
        var taskService = new Mock<ITaskService>();
        taskService.Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);
        var controller = CreateController(taskService.Object, userId);

        var result = await controller.GetAll(CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(tasks, response.Value);
        taskService.Verify(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task Create_WhenServiceCreatesTask_ReturnsCreatedAtActionResponse()
    {
        var userId = Guid.NewGuid();
        var task = new TaskResponseDto(
            Guid.NewGuid(), "Task", null, 1, "Open", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);
        var request = new CreateTaskRequestDto
        {
            Title = "Task",
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        var taskService = new Mock<ITaskService>();
        taskService.Setup(x => x.CreateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        var controller = CreateController(taskService.Object, userId);

        var result = await controller.Create(request, CancellationToken.None);

        var response = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TasksController.GetById), response.ActionName);
        Assert.Equal(task.Id, response.RouteValues!["id"]);
        Assert.Same(task, response.Value);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task Delete_WhenServiceDeletesTask_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        var controller = CreateController(taskService.Object, userId);

        var result = await controller.Delete(taskId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        taskService.Verify(x => x.DeleteAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetById_WhenTaskExists_ReturnsOkResponse()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskResponseDto(taskId, "Task", null, 1, "Open", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, null);
        var taskService = new Mock<ITaskService>();
        taskService.Setup(x => x.GetByIdAsync(taskId, MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        var controller = CreateController(taskService.Object, MockedData.UserId);

        var result = await controller.GetById(taskId, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, response.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_WhenTaskExists_ReturnsOkResponse()
    {
        var taskId = Guid.NewGuid();
        var request = MockedData.UpdateRequest();
        var task = new TaskResponseDto(taskId, "Updated", request.Description, 1, "Open",
            request.DueDate!.Value, DateTimeOffset.UtcNow, null);
        var taskService = new Mock<ITaskService>();
        taskService.Setup(x => x.UpdateAsync(taskId, MockedData.UserId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        var controller = CreateController(taskService.Object, MockedData.UserId);

        var result = await controller.Update(taskId, request, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, response.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetTaskStatus_WhenStatusesExist_ReturnsOkResponse()
    {
        var statuses = new List<TaskStatusResponseDto> { new(1, "Open") };
        var taskService = new Mock<ITaskService>();
        taskService.Setup(x => x.GetAllTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(statuses);
        var controller = CreateController(taskService.Object, MockedData.UserId);

        var result = await controller.GetTaskStatus(CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(statuses, response.Value);
    }

    [Fact]
    [Trait("Category", "Task")]
    public async Task GetAll_WhenUserIdClaimIsInvalid_ThrowsInvalidOperationException()
    {
        var controller = CreateController(Mock.Of<ITaskService>(), Guid.Empty, "not-a-guid");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetAll(CancellationToken.None));

        Assert.Equal("The authenticated user ID claim is missing or invalid.", exception.Message);
    }

    private static TasksController CreateController(
        ITaskService taskService,
        Guid userId,
        string? claimValue = null)
    {
        var claims = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, claimValue ?? userId.ToString())],
            "TestAuthentication");
        return new TasksController(taskService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claims) }
            }
        };
    }
}
