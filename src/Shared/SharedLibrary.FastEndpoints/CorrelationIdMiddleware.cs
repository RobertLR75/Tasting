using Microsoft.AspNetCore.Http;

namespace SharedLibrary.FastEndpoints;

internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    internal const string ItemKey = "CorrelationId";
    internal const string HeaderName = "X-Correlation-Id";

    public Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var headerValue)
            ? headerValue.ToString().Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
