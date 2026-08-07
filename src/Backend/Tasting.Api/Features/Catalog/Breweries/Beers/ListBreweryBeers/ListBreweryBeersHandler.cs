using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed class ListBreweryBeersHandler(CatalogDbContext dbContext) : IRequestHandler<ListBreweryBeersQuery, ListBreweryBeersResult>
{
    public async Task<ListBreweryBeersResult> HandleAsync(ListBreweryBeersQuery request, CancellationToken ct = default)
    {
        var breweryExists = await dbContext.Breweries
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BreweryId, ct);

        if (!breweryExists)
        {
            throw new ServiceNotFoundException($"Brewery '{request.BreweryId}' was not found.");
        }

        var beers = await dbContext.Beers
            .AsNoTracking()
            .Where(b => b.BreweryId == request.BreweryId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        var styleIds = beers.Select(b => b.BeerStyleId).Distinct().ToList();
        var typeIds = beers.Select(b => b.BeerTypeId).Distinct().ToList();

        var breweryName = await dbContext.Breweries.AsNoTracking()
            .Where(b => b.Id == request.BreweryId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var styleNames = await dbContext.BeerStyles.AsNoTracking()
            .Where(s => styleIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var typeNames = await dbContext.BeerTypes.AsNoTracking()
            .Where(t => typeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        var items = beers.Select(b => new ListBreweryBeersItem(
            b.Id,
            b.Name,
            b.IsActive,
            b.BreweryId,
            breweryName,
            b.BeerStyleId,
            styleNames.TryGetValue(b.BeerStyleId, out var styleName) ? styleName : string.Empty,
            b.BeerTypeId,
            typeNames.TryGetValue(b.BeerTypeId, out var typeName) ? typeName : string.Empty)).ToList();

        return new ListBreweryBeersResult(items);
    }
}
