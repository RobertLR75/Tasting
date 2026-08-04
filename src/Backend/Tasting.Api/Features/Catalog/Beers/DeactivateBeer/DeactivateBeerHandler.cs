using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Beers.DeactivateBeer;

public sealed class DeactivateBeerHandler(CatalogDbContext dbContext) : IRequestHandler<DeactivateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(DeactivateBeerCommand request, CancellationToken ct = default)
    {
        var beer = await dbContext.Beers
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");

        if (!beer.IsActive)
        {
            return beer;
        }

        beer.IsActive = false;
        beer.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return beer;
    }
}
