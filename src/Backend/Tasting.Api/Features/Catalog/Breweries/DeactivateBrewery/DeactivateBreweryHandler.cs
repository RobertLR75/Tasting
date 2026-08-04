using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;

public sealed class DeactivateBreweryHandler(CatalogDbContext dbContext) : IRequestHandler<DeactivateBreweryCommand, Brewery>
{
    public async Task<Brewery> HandleAsync(DeactivateBreweryCommand request, CancellationToken ct = default)
    {
        var brewery = await dbContext.Breweries
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");

        if (!brewery.IsActive && !await dbContext.Beers.AnyAsync(x => x.BreweryId == request.Id && x.IsActive, ct))
        {
            return brewery;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        brewery.IsActive = false;
        brewery.UpdatedAt = DateTimeOffset.UtcNow;

        var beers = await dbContext.Beers
            .Where(x => x.BreweryId == request.Id && x.IsActive)
            .ToListAsync(ct);

        foreach (var beer in beers)
        {
            beer.IsActive = false;
            beer.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return brewery;
    }
}
