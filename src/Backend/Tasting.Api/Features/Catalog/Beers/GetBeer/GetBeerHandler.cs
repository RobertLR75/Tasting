using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Beers.GetBeer;

public sealed class GetBeerHandler(CatalogDbContext dbContext) : IRequestHandler<GetBeerQuery, BeerResponse>
{
    public async Task<BeerResponse> HandleAsync(GetBeerQuery request, CancellationToken ct = default)
    {
        var beer = await dbContext.Beers
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BeerResponse(x.Id, x.BreweryId, x.BeerStyleId, x.BeerTypeId, x.Name, x.IsActive, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return beer ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");
    }
}
