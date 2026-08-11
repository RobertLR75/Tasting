using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed class ListBreweriesHandler(IPersistenceService<Brewery> breweries) : IRequestHandler<ListBreweriesQuery, ListBreweriesResponse>
{
    public async Task<ListBreweriesResponse> HandleAsync(ListBreweriesQuery request, CancellationToken ct = default)
    {
        var items = await breweries.SearchAsync(new AllBreweriesSpecification(request.IncludeInactive), ct);
        return new ListBreweriesResponse
        {
            Breweries = items.Select(x => new BrewerySummaryResponse(x.Id, x.Name, x.IsActive, x.CreatedAt, x.UpdatedAt)).ToList()
        };
    }
}
