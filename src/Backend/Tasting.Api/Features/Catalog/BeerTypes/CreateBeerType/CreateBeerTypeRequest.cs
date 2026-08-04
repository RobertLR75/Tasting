using System.ComponentModel.DataAnnotations;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed class CreateBeerTypeRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
