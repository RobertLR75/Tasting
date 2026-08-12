using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;

public sealed class DeactivateBreweryHandler(
    IPersistenceService<Brewery> breweries,
    IPersistenceService<Beer> beers,
    ICatalogDeactivationService catalog) : IRequestHandler<DeactivateBreweryCommand, Brewery>
{
    public async Task<Brewery> HandleAsync(DeactivateBreweryCommand request, CancellationToken ct = default)
    {
        var brewery = await breweries.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");
        var activeBeers = await beers.SearchAsync(new ActiveBeersForBrewerySpecification(request.Id), ct);

        if (!brewery.IsActive && activeBeers.Count == 0)
        {
            return brewery;
        }

        var updatedAt = DateTimeOffset.UtcNow;
        brewery.IsActive = false;
        brewery.UpdatedAt = updatedAt;
        foreach (var beer in activeBeers)
        {
            beer.IsActive = false;
            beer.UpdatedAt = updatedAt;
        }

        await catalog.SaveDeactivationAsync(brewery, activeBeers, ct);
        return brewery;
    }
}
