using Tasting.Api.Features.Catalog.BeerTypes;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesResponse
{
    public List<BeerTypeSummaryResponse> BeerTypes { get; init; } = [];
}
