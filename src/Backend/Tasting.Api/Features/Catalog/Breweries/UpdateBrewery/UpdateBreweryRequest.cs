using System.ComponentModel.DataAnnotations;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed class UpdateBreweryRequest
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
