using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using SharedLibrary.Services.Exceptions;
using ErrorContractResponse = SharedLibrary.FastEndpoints.Contracts.ErrorResponse;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace SharedLibrary.FastEndpoints;


public static class FastEndPointsExtensions
{
    public static void ConfigureFastEndPoints(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddFastEndpoints();
    }

    public static void UseEndpoints(
        this WebApplication app,
        string routePrefix = "api/v1")
    {
        app.UseHttpsRedirection();

        app.UseFastEndpoints(config =>
        {
            if (!string.IsNullOrWhiteSpace(routePrefix))
            {
                config.Endpoints.RoutePrefix = routePrefix;
            }
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
    }

    public static async Task WriteErrorResponseAsync(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, code, message) = MapException(exception);
        var correlationId = context.GetCorrelationId();

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new ErrorContractResponse(code, message, correlationId),
            context.RequestAborted);
    }

    private static (int statusCode, string code, string message) MapException(Exception? exception)
    {
        if (exception is null)
        {
            return (StatusCodes.Status500InternalServerError, "internal_error", "An unexpected error occurred.");
        }

        return exception switch
        {
            ServiceNotFoundException => (StatusCodes.Status404NotFound, "not_found", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "conflict", exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, "forbidden", exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, "validation_error", exception.Message),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "validation_error", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "internal_error", "An unexpected error occurred.")
        };
    }

    private static string GetCorrelationId(this HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationIdObj) &&
            correlationIdObj is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        return context.TraceIdentifier;
    }
}