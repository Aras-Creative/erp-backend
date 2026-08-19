using ArasERP.BuildingBlocks.Presentation;
using ArasERP.BuildingBlocks.Presentation.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ArasERP.Tests.Unit.Shared;

public class ApiResponseTests
{
    [Fact]
    public void Success_SetsDataAndRequestIdFromContext()
    {
        var context = new DefaultHttpContext();
        context.Items[RequestIdMiddleware.ItemsKey] = "req-123";

        var response = ApiResponse.Success(context, "value", "Done");

        response.Data.Should().Be("value");
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Done");
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.RequestId.Should().Be("req-123");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Success_FallsBackToTraceIdentifierWhenRequestIdNotSet()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-456";

        var response = ApiResponse.Success(context, "value");

        response.RequestId.Should().Be("trace-456");
    }

    [Fact]
    public void Fail_SetsMessageAndErrorsWithNullData()
    {
        var context = new DefaultHttpContext();
        context.Items[RequestIdMiddleware.ItemsKey] = "req-789";

        var response = ApiResponse.Fail<object>(
            context,
            StatusCodes.Status400BadRequest,
            "Validation failed",
            ["invalid name"]
        );

        response.Data.Should().BeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().ContainSingle("invalid name");
        response.RequestId.Should().Be("req-789");
    }
}
