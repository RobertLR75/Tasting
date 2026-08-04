using System.ComponentModel.DataAnnotations;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed class CreateBeerStyleRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
