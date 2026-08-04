using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed class UpdateBreweryHandler(CatalogDbContext dbContext) : IRequestHandler<UpdateBreweryCommand, Brewery>
{
    public async Task<Brewery> HandleAsync(UpdateBreweryCommand request, CancellationToken ct = default)
    {
        var brewery = await dbContext.Breweries
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ServiceNotFoundException($"Brewery '{request.Id}' was not found.");

        brewery.Name = request.Name.Trim();
        brewery.IsActive = request.IsActive;
        brewery.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return brewery;
    }
}
