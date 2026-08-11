using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.GetBrewery;

public sealed class GetBreweryHandler(IPersistenceService<Brewery> breweries) : IRequestHandler<GetBreweryQuery, BreweryResponse>
{
    public async Task<BreweryResponse> HandleAsync(GetBreweryQuery request, CancellationToken ct = default)
    {
        var brewery = await breweries.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");
        return new BreweryResponse(brewery.Id, brewery.Name, brewery.IsActive, brewery.CreatedAt, brewery.UpdatedAt);
    }
}
