using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed class ListBreweriesHandler(CatalogDbContext dbContext) : IRequestHandler<ListBreweriesQuery, ListBreweriesResponse>
{
    public async Task<ListBreweriesResponse> HandleAsync(ListBreweriesQuery request, CancellationToken ct = default)
    {
        var query = dbContext.Breweries
            .AsNoTracking()
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        var breweries = await query
            .OrderBy(x => x.Name)
            .Select(x => new BrewerySummaryResponse(x.Id, x.Name, x.IsActive, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(ct);

        return new ListBreweriesResponse { Breweries = breweries };
    }
}
