using ArasERP.BuildingBlocks.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ArasERP.BuildingBlocks.Presentation.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                ApiResponse.Fail<object>(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Validation failed",
                    ex.Errors
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                ApiResponse.Fail<object>(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                )
            );
        }
    }
}
