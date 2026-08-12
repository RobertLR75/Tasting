using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryHandler(IPersistenceService<Brewery> breweries) : IRequestHandler<CreateBreweryCommand, Brewery>
{
    public async Task<Brewery> HandleAsync(CreateBreweryCommand request, CancellationToken ct = default)
    {
        var brewery = new Brewery
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await breweries.CreateAsync(brewery, ct);
        return brewery;
    }
}
