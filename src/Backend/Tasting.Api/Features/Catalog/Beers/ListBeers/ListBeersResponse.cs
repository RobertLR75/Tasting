namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersResponse
{
    public List<ListBeersResponseItem> Beers { get; init; } = [];
}

public sealed class ListBeersResponseItem
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
