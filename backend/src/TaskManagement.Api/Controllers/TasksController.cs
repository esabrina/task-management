using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetAllAsync(GetUserId(), cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<TaskResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> Create(
        CreateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(GetUserId(), request, cancellationToken);
        return CreatedAtAction("GetById", new { id = task.Id }, task);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetByIdAsync(id, GetUserId(), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaskResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> Update(
        Guid id,
        UpdateTaskRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await taskService.UpdateAsync(id, GetUserId(), request, cancellationToken));
    }

    [HttpGet("status")]
    [ProducesResponseType<IReadOnlyList<TaskStatusResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TaskStatusResponseDto>>> GetTaskStatus(CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetAllTaskStatusesAsync(cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, GetUserId(), cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : throw new InvalidOperationException("The authenticated user ID claim is missing or invalid.");
    }
}
