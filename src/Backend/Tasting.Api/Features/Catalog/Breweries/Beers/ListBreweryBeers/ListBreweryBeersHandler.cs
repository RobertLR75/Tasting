using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed class ListBreweryBeersHandler(
    IPersistenceService<Brewery> breweries,
    IPersistenceService<Beer> beers) : IRequestHandler<ListBreweryBeersQuery, ListBreweryBeersResult>
{
    public async Task<ListBreweryBeersResult> HandleAsync(ListBreweryBeersQuery request, CancellationToken ct = default)
    {
        if (await breweries.GetAsync(request.BreweryId, ct) is null)
        {
            throw new ServiceNotFoundException($"Brewery '{request.BreweryId}' was not found.");
        }

        var entities = await beers.SearchAsync(new BeersWithCatalogSpecification(false, request.BreweryId), ct);
        var items = entities.Select(b => new ListBreweryBeersItem(
            b.Id,
            b.Name,
            b.IsActive,
            b.BreweryId,
            b.Brewery?.Name ?? string.Empty,
            b.BeerStyleId,
            b.BeerStyle?.Name ?? string.Empty,
            b.BeerTypeId,
            b.BeerType?.Name ?? string.Empty)).ToList();

        return new ListBreweryBeersResult(items);
    }
}
