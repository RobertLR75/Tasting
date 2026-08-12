using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.DeactivateBeer;

public sealed class DeactivateBeerHandler(IPersistenceService<Beer> beers) : IRequestHandler<DeactivateBeerCommand, Beer>
{
    public async Task<Beer> HandleAsync(DeactivateBeerCommand request, CancellationToken ct = default)
    {
        var beer = await beers.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Beer '{request.Id}' was not found.");

        if (!beer.IsActive)
        {
            return beer;
        }

        beer.IsActive = false;
        await beers.UpdateAsync(beer, ct);
        return beer;
    }
}
