using System.Text.Json.Serialization;

namespace SharedLibrary.FastEndpoints.Contracts;

public sealed record ErrorResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("correlationId")] string CorrelationId);
