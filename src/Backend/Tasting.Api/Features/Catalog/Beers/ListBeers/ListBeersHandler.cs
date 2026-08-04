using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersHandler(CatalogDbContext dbContext) : IRequestHandler<ListBeersQuery, ListBeersResult>
{
    public async Task<ListBeersResult> HandleAsync(ListBeersQuery request, CancellationToken ct = default)
    {
        var query = dbContext.Beers
            .AsNoTracking()
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        var beers = await query
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var breweryIds = beers.Select(b => b.BreweryId).Distinct().ToList();
        var styleIds = beers.Select(b => b.BeerStyleId).Distinct().ToList();
        var typeIds = beers.Select(b => b.BeerTypeId).Distinct().ToList();

        var breweryNames = await dbContext.Breweries.AsNoTracking()
            .Where(b => breweryIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);
        var styleNames = await dbContext.BeerStyles.AsNoTracking()
            .Where(s => styleIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);
        var typeNames = await dbContext.BeerTypes.AsNoTracking()
            .Where(t => typeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        var items = beers.Select(x => new ListBeersItem(
            x.Id,
            x.Name,
            x.IsActive,
            x.BreweryId,
            breweryNames.TryGetValue(x.BreweryId, out var breweryName) ? breweryName : string.Empty,
            x.BeerStyleId,
            styleNames.TryGetValue(x.BeerStyleId, out var styleName) ? styleName : string.Empty,
            x.BeerTypeId,
            typeNames.TryGetValue(x.BeerTypeId, out var typeName) ? typeName : string.Empty)).ToList();

        return new ListBeersResult(items);
    }
}
