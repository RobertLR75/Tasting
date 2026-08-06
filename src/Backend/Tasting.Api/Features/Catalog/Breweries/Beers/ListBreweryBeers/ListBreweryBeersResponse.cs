namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed class ListBreweryBeersResponse
{
    public List<ListBreweryBeersResponseItem> Beers { get; init; } = [];
}

public sealed class ListBreweryBeersResponseItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid BreweryId { get; init; }
    public string BreweryName { get; init; } = string.Empty;
    public Guid BeerStyleId { get; init; }
    public string BeerStyleName { get; init; } = string.Empty;
    public Guid BeerTypeId { get; init; }
    public string BeerTypeName { get; init; } = string.Empty;
}
