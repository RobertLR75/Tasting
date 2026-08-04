namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerResponse
{
    public Guid Id { get; init; }
    public Guid BreweryId { get; init; }
    public Guid BeerStyleId { get; init; }
    public Guid BeerTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
