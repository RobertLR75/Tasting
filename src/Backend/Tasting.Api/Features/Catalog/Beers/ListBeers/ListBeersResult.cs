namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed record ListBeersResult(IReadOnlyCollection<ListBeersItem> Beers);

public sealed record ListBeersItem(
    Guid Id,
    string Name,
    bool IsActive,
    Guid BreweryId,
    string BreweryName,
    Guid BeerStyleId,
    string BeerStyleName,
    Guid BeerTypeId,
    string BeerTypeName);
