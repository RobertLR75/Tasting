using System.ComponentModel.DataAnnotations;

namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerRequest
{
    [Required]
    public Guid BreweryId { get; init; }

    [Required]
    public Guid BeerStyleId { get; init; }

    [Required]
    public Guid BeerTypeId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
