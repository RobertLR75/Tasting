using System.ComponentModel.DataAnnotations;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
