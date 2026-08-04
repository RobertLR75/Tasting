using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Beers.UpdateBeer;

public sealed class UpdateBeerHandler(CatalogDbContext dbContext) : IRequestHandler<UpdateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(UpdateBeerCommand request, CancellationToken ct = default)
    {
        var beer = await dbContext.Beers
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");

        var brewery = await dbContext.Breweries
            .FirstOrDefaultAsync(x => x.Id == request.BreweryId, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.BreweryId}' was not found.");

        if (!brewery.IsActive)
        {
            throw new ConflictException($"Brewery '{request.BreweryId}' is inactive.");
        }

        var beerStyleExists = await dbContext.BeerStyles
            .AnyAsync(x => x.Id == request.BeerStyleId, ct);
        if (!beerStyleExists)
        {
            throw new ServiceNotFoundException($"BeerStyle '{request.BeerStyleId}' was not found.");
        }

        var beerTypeExists = await dbContext.BeerTypes
            .AnyAsync(x => x.Id == request.BeerTypeId, ct);
        if (!beerTypeExists)
        {
            throw new ServiceNotFoundException($"BeerType '{request.BeerTypeId}' was not found.");
        }

        var normalizedName = request.Name.Trim().ToLowerInvariant();
        var duplicateExists = await dbContext.Beers
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.BreweryId == request.BreweryId &&
                     x.Name.ToLowerInvariant() == normalizedName,
                ct);

        if (duplicateExists)
        {
            throw new ConflictException("A beer with this name already exists for this brewery.");
        }

        beer.BreweryId = request.BreweryId;
        beer.BeerStyleId = request.BeerStyleId;
        beer.BeerTypeId = request.BeerTypeId;
        beer.Name = request.Name.Trim();
        beer.IsActive = request.IsActive;
        beer.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return beer;
    }
}
