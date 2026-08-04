using Tasting.Api.Features.Catalog.Breweries;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed class ListBreweriesResponse
{
    public List<BrewerySummaryResponse> Breweries { get; init; } = [];
}
