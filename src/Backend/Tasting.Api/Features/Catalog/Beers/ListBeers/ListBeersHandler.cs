using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersHandler(IPersistenceService<Beer> beers) : IRequestHandler<ListBeersQuery, ListBeersResult>
{
    public async Task<ListBeersResult> HandleAsync(ListBeersQuery request, CancellationToken ct = default)
    {
        var entities = await beers.SearchAsync(new BeersWithCatalogSpecification(request.IncludeInactive), ct);
        var items = entities.Select(x => new ListBeersItem(
            x.Id,
            x.Name,
            x.IsActive,
            x.BreweryId,
            x.Brewery?.Name ?? string.Empty,
            x.BeerStyleId,
            x.BeerStyle?.Name ?? string.Empty,
            x.BeerTypeId,
            x.BeerType?.Name ?? string.Empty)).ToList();

        return new ListBeersResult(items);
    }
}
