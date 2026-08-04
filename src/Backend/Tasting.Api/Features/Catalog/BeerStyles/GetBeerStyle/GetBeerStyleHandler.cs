using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;

public sealed class GetBeerStyleHandler(CatalogDbContext dbContext) : IRequestHandler<GetBeerStyleQuery, BeerStyleResponse>
{
    public async Task<BeerStyleResponse> HandleAsync(GetBeerStyleQuery request, CancellationToken ct = default)
    {
        var style = await dbContext.BeerStyles
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BeerStyleResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return style ?? throw new ServiceNotFoundException($"BeerStyle '{request.Id}' was not found.");
    }
}
