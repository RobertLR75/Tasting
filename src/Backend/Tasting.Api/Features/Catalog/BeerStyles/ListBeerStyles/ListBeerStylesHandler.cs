using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesHandler(CatalogDbContext dbContext) : IRequestHandler<ListBeerStylesQuery, ListBeerStylesResponse>
{
    public async Task<ListBeerStylesResponse> HandleAsync(ListBeerStylesQuery request, CancellationToken ct = default)
    {
        var items = await dbContext.BeerStyles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BeerStyleSummaryResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(ct);

        return new ListBeerStylesResponse { BeerStyles = items };
    }
}
