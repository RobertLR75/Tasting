using SharedLibrary.Interfaces;

namespace Tasting.Api.Features.Catalog.Domain;

public sealed class Beer : IEntity
{
    public Guid Id { get; set; }
    public Guid BreweryId { get; set; }
    public Guid BeerStyleId { get; set; }
    public Guid BeerTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Brewery Brewery { get; set; } = null!;
    public BeerStyle BeerStyle { get; set; } = null!;
    public BeerType BeerType { get; set; } = null!;
}
