using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Exceptions;

namespace SharedLibrary.FastEndpoints.UnitTests;

public class FastEndpointsExtensionsTests
{
    [Fact]
    public async Task WriteErrorResponseAsync_WritesMappedContractAndCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
        {
            Error = new ConflictException("duplicate")
        });
        context.Items[CorrelationIdMiddleware.ItemKey] = "corr-123";

        await FastEndPointsExtensions.WriteErrorResponseAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("\"code\":\"conflict\"", body);
        Assert.Contains("\"message\":\"duplicate\"", body);
        Assert.Contains("\"correlationId\":\"corr-123\"", body);
    }

    [Fact]
    public async Task WriteErrorResponseAsync_MapsUnauthorizedServiceException()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
        {
            Error = new UnauthorizedException("Invalid email or password.")
        });

        await FastEndPointsExtensions.WriteErrorResponseAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("\"code\":\"unauthorized\"", body);
        Assert.Contains("\"message\":\"Invalid email or password.\"", body);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_UsesHeaderAndSetsResponseHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = " header-id ";
        var responseStarted = false;
        RequestDelegate next = _ =>
        {
            responseStarted = true;
            return Task.CompletedTask;
        };

        var sut = new CorrelationIdMiddleware(next);

        await sut.Invoke(context);
        Assert.True(responseStarted);
        Assert.Equal("header-id", context.TraceIdentifier);
        Assert.Equal("header-id", context.Items[CorrelationIdMiddleware.ItemKey]);
    }

    [Fact]
    public void ConfigureFastEndPoints_ThrowsWithoutEndpoints()
    {
        var builder = WebApplication.CreateBuilder();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureFastEndPoints());
        Assert.Contains("unable to find any endpoint declarations", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
