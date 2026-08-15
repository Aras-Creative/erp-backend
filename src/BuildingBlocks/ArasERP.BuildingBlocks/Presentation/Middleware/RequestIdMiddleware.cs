using Microsoft.AspNetCore.Http;

namespace ArasERP.BuildingBlocks.Presentation.Middleware;

public sealed class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-Id";

    public const string ItemsKey = "RequestId";

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId =
            context.Request.Headers[HeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString("N");

        context.Items[ItemsKey] = requestId;
        context.TraceIdentifier = requestId;
        context.Response.Headers[HeaderName] = requestId;

        await _next(context);
    }
}
