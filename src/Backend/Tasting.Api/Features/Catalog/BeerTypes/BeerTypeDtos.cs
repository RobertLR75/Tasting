namespace Tasting.Api.Features.Catalog.BeerTypes;

public sealed record BeerTypeResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record BeerTypeSummaryResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
