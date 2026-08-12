using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.UpdateBeer;

public sealed class UpdateBeerHandler(
    IPersistenceService<Brewery> breweries,
    IPersistenceService<BeerStyle> styles,
    IPersistenceService<BeerType> types,
    IPersistenceService<Beer> beers) : IRequestHandler<UpdateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(UpdateBeerCommand request, CancellationToken ct = default)
    {
        var beer = await beers.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");

        var brewery = await breweries.GetAsync(request.BreweryId, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.BreweryId}' was not found.");

        if (!brewery.IsActive)
        {
            throw new ConflictException($"Brewery '{request.BreweryId}' is inactive.");
        }

        if (await styles.GetAsync(request.BeerStyleId, ct) is null)
        {
            throw new ServiceNotFoundException($"BeerStyle '{request.BeerStyleId}' was not found.");
        }

        if (await types.GetAsync(request.BeerTypeId, ct) is null)
        {
            throw new ServiceNotFoundException($"BeerType '{request.BeerTypeId}' was not found.");
        }

        if ((await beers.SearchAsync(new BeerNameWithinBrewerySpecification(request.BreweryId, request.Name, request.Id), ct)).Count > 0)
        {
            throw new ConflictException("A beer with this name already exists for this brewery.");
        }

        beer.BreweryId = request.BreweryId;
        beer.BeerStyleId = request.BeerStyleId;
        beer.BeerTypeId = request.BeerTypeId;
        beer.Name = request.Name.Trim();
        beer.IsActive = request.IsActive;
        await beers.UpdateAsync(beer, ct);
        return beer;
    }
}
