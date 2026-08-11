using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;

public sealed class GetBeerTypeHandler(IPersistenceService<BeerType> types) : IRequestHandler<GetBeerTypeQuery, BeerTypeResponse>
{
    public async Task<BeerTypeResponse> HandleAsync(GetBeerTypeQuery request, CancellationToken ct = default)
    {
        var type = await types.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"BeerType '{request.Id}' was not found.");
        return new BeerTypeResponse(type.Id, type.Name, type.CreatedAt, type.UpdatedAt);
    }
}
