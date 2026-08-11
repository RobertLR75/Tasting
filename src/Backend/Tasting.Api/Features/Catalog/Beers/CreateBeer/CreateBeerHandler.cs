using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerHandler(
    IPersistenceService<Brewery> breweries,
    IPersistenceService<BeerStyle> styles,
    IPersistenceService<BeerType> types,
    IPersistenceService<Beer> beers) : IRequestHandler<CreateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(CreateBeerCommand request, CancellationToken ct = default)
    {
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

        if ((await beers.SearchAsync(new BeerNameWithinBrewerySpecification(request.BreweryId, request.Name), ct)).Count > 0)
        {
            throw new ConflictException("A beer with this name already exists for this brewery.");
        }

        var beer = new Beer
        {
            Id = Guid.CreateVersion7(),
            BreweryId = request.BreweryId,
            BeerStyleId = request.BeerStyleId,
            BeerTypeId = request.BeerTypeId,
            Name = request.Name.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await beers.CreateAsync(beer, ct);
        return beer;
    }
}
