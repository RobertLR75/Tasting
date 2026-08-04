namespace Tasting.Api.Features.Catalog.BeerStyles;

public sealed record BeerStyleResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record BeerStyleSummaryResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
