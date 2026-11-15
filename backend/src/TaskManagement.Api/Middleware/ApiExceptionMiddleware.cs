using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TaskManagement.Application.Exceptions;

namespace TaskManagement.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, exception.Message);
        }
        catch (UnauthorizedException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, exception.Message);
        }
        catch (RequestValidationException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(
                exception.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred while processing the request.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail
        });
    }
}
