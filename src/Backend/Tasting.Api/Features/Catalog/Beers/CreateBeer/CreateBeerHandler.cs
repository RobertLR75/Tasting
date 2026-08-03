using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerHandler(CatalogDbContext dbContext) : IRequestHandler<CreateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(CreateBeerCommand request, CancellationToken ct = default)
    {
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
                x => x.BreweryId == request.BreweryId &&
                     x.Name.ToLower() == normalizedName,
                ct);

        if (duplicateExists)
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

        dbContext.Beers.Add(beer);
        await dbContext.SaveChangesAsync(ct);
        return beer;
    }
}
