using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.GetBeer;

public sealed class GetBeerHandler(IPersistenceService<Beer> beers) : IRequestHandler<GetBeerQuery, BeerResponse>
{
    public async Task<BeerResponse> HandleAsync(GetBeerQuery request, CancellationToken ct = default)
    {
        var beer = await beers.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");
        return new BeerResponse(beer.Id, beer.BreweryId, beer.BeerStyleId, beer.BeerTypeId, beer.Name, beer.IsActive, beer.CreatedAt, beer.UpdatedAt);
    }
}
