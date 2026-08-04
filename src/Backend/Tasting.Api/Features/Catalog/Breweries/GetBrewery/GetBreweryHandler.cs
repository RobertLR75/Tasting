using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.GetBrewery;

public sealed class GetBreweryHandler(CatalogDbContext dbContext) : IRequestHandler<GetBreweryQuery, BreweryResponse>
{
    public async Task<BreweryResponse> HandleAsync(GetBreweryQuery request, CancellationToken ct = default)
    {
        var brewery = await dbContext.Breweries
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BreweryResponse(x.Id, x.Name, x.IsActive, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return brewery ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");
    }
}
