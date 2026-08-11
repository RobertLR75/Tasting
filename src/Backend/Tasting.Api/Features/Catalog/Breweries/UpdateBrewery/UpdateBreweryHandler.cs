using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed class UpdateBreweryHandler(IPersistenceService<Brewery> breweries) : IRequestHandler<UpdateBreweryCommand, Brewery>
{
    public async Task<Brewery> HandleAsync(UpdateBreweryCommand request, CancellationToken ct = default)
    {
        var brewery = await breweries.GetAsync(request.Id, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");

        brewery.Name = request.Name.Trim();
        brewery.IsActive = request.IsActive;
        await breweries.UpdateAsync(brewery, ct);
        return brewery;
    }
}
