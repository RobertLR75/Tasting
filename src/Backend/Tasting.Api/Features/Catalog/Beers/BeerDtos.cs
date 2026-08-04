namespace Tasting.Api.Features.Catalog.Beers;

public sealed record BeerResponse(
    Guid Id,
    Guid BreweryId,
    Guid BeerStyleId,
    Guid BeerTypeId,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
