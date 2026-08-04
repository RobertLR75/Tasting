using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryHandler(CatalogDbContext dbContext) : IRequestHandler<CreateBreweryCommand, Brewery>
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

        dbContext.Breweries.Add(brewery);
        await dbContext.SaveChangesAsync(ct);
        return brewery;
    }
}
