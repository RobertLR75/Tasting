namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
