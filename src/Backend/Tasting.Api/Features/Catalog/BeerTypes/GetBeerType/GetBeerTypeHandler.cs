using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;

public sealed class GetBeerTypeHandler(CatalogDbContext dbContext) : IRequestHandler<GetBeerTypeQuery, BeerTypeResponse>
{
    public async Task<BeerTypeResponse> HandleAsync(GetBeerTypeQuery request, CancellationToken ct = default)
    {
        var type = await dbContext.BeerTypes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BeerTypeResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return type ?? throw new ServiceNotFoundException($"BeerType '{request.Id}' was not found.");
    }
}
