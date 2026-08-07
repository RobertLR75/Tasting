namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed record ListBreweryBeersResult(IReadOnlyCollection<ListBreweryBeersItem> Beers);

public sealed record ListBreweryBeersItem(
    Guid Id,
    string Name,
    bool IsActive,
    Guid BreweryId,
    string BreweryName,
    Guid BeerStyleId,
    string BeerStyleName,
    Guid BeerTypeId,
    string BeerTypeName);
