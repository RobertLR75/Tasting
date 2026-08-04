using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesHandler(CatalogDbContext dbContext) : IRequestHandler<ListBeerTypesQuery, ListBeerTypesResponse>
{
    public async Task<ListBeerTypesResponse> HandleAsync(ListBeerTypesQuery request, CancellationToken ct = default)
    {
        var items = await dbContext.BeerTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BeerTypeSummaryResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(ct);

        return new ListBeerTypesResponse { BeerTypes = items };
    }
}
