using Microsoft.AspNetCore.Builder;

namespace ArasERP.BuildingBlocks.Presentation.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestId(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestIdMiddleware>();

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
