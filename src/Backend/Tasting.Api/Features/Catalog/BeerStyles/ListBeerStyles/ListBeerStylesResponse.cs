using Tasting.Api.Features.Catalog.BeerStyles;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesResponse
{
    public List<BeerStyleSummaryResponse> BeerStyles { get; init; } = [];
}
