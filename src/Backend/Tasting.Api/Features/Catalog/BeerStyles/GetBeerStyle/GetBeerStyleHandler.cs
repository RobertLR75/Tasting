using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;

public sealed class GetBeerStyleHandler(IPersistenceService<BeerStyle> styles) : IRequestHandler<GetBeerStyleQuery, BeerStyleResponse>
{
    public async Task<BeerStyleResponse> HandleAsync(GetBeerStyleQuery request, CancellationToken ct = default)
    {
        var style = await styles.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"BeerStyle '{request.Id}' was not found.");
        return new BeerStyleResponse(style.Id, style.Name, style.CreatedAt, style.UpdatedAt);
    }
}
