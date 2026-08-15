using ArasERP.BuildingBlocks.Presentation.Middleware;
using Microsoft.AspNetCore.Http;

namespace ArasERP.BuildingBlocks.Presentation;

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(
        HttpContext context,
        T data,
        string? message = null,
        int statusCode = StatusCodes.Status200OK
    ) =>
        new()
        {
            Data = data,
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Errors = null,
            RequestId = GetRequestId(context),
            Timestamp = DateTimeOffset.UtcNow,
        };

    public static ApiResponse<T> Fail<T>(
        HttpContext context,
        int statusCode,
        string? message,
        IReadOnlyList<string>? errors = null
    ) =>
        new()
        {
            Data = default,
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            RequestId = GetRequestId(context),
            Timestamp = DateTimeOffset.UtcNow,
        };

    private static string? GetRequestId(HttpContext context) =>
        context.Items.TryGetValue(RequestIdMiddleware.ItemsKey, out var requestId)
            ? requestId as string
            : context.TraceIdentifier;
}

public sealed class ApiResponse<T>
{
    public T? Data { get; init; }

    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string? Message { get; init; }

    public IReadOnlyList<string>? Errors { get; init; }

    public string? RequestId { get; init; }

    public DateTimeOffset Timestamp { get; init; }
}
