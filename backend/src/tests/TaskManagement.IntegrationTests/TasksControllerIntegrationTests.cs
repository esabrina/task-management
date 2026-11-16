using System.Net;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs;

namespace TaskManagement.IntegrationTests;

public sealed class TasksControllerIntegrationTests(CustomWebApplicationFactory factory)
    : AuthenticatedIntegrationTestBase(factory), IClassFixture<CustomWebApplicationFactory>
{
    private static CreateTaskRequestDto NewTaskRequest(string title = "Integration task", 
        int statusId = 1) => new()
    {
        Title = title,
        Description = "Integration test description",
        StatusId = statusId,
        DueDate = DateTimeOffset.UtcNow.AddDays(7)
    };

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskStatuses_ReturnsSeededStatuses()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/tasks/status");
        var statuses = await response.Content.ReadFromJsonAsync<TaskStatusResponseDto[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(statuses);
        Assert.Equal(["To-do", "Doing", "Done", "Cancelled"], statuses.Select(status => status.Name));
    }

    [Fact]
    public async Task Create_ReturnsCreatedTask()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var request = NewTaskRequest();

        var createResponse = await client.PostAsJsonAsync("/api/tasks", request);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdTask);
        Assert.Equal(request.Title, createdTask.Title);
        Assert.Equal(request.StatusId, createdTask.StatusId);
    }

    [Fact]
    public async Task GetById_ReturnsCreatedTask()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var createdTask = await CreateTaskAsync(client);

        var response = await client.GetAsync($"/api/tasks/{createdTask.Id}");
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(task);
        Assert.Equal(createdTask.Id, task.Id);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedTask()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var createdTask = await CreateTaskAsync(client);
        var request = NewTaskRequest("Updated integration task", statusId: 2);

        var response = await client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", request);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedTask);
        Assert.Equal(request.Title, updatedTask.Title);
        Assert.Equal(request.StatusId, updatedTask.StatusId);
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var createdTask = await CreateTaskAsync(client);

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{createdTask.Id}");
        var getResponse = await client.GetAsync($"/api/tasks/{createdTask.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenTaskBelongsToAnotherUser_ReturnsNotFound()
    {
        using var ownerClient = await CreateAuthenticatedClientAsync();
        using var otherClient = await CreateAuthenticatedClientAsync();
        var createdTask = await CreateTaskAsync(ownerClient);

        var response = await otherClient.GetAsync($"/api/tasks/{createdTask.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ReturnsBadRequest()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var request = new
        {
            Title = string.Empty,
            Description = "Invalid task",
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var response = await client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<TaskResponseDto> CreateTaskAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", NewTaskRequest());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TaskResponseDto>()
            ?? throw new InvalidOperationException("The create task response was empty.");
    }
}
