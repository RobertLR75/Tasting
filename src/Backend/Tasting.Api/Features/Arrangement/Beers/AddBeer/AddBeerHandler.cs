using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Arrangement.Beers.AddBeer;

public sealed class AddBeerHandler(
    ArrangementDbContext dbContext,
    CatalogDbContext catalogDbContext)
    : IRequestHandler<AddBeerCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        AddBeerCommand request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .Include(a => a.Participants)
            .Include(a => a.Beers)
            .FirstOrDefaultAsync(a => a.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        if (arrangement.Status != ArrangementStatus.Created)
        {
            throw new ConflictException(
                "Beers can only be added when arrangement is in Created status.");
        }

        if (arrangement.RowVersion != request.RowVersion)
        {
            throw new ConflictException(
                "Arrangement has been modified by another request. Please reload and retry.");
        }

        var beerExists = await catalogDbContext.Beers
            .AnyAsync(b => b.Id == request.BeerId, ct);
        if (!beerExists)
        {
            throw new ServiceNotFoundException($"Beer '{request.BeerId}' was not found.");
        }

        var alreadyAdded = arrangement.Beers
            .Any(b => b.BeerId == request.BeerId);
        if (alreadyAdded)
        {
            throw new ConflictException(
                "This beer is already added to the arrangement.");
        }

        arrangement.Beers.Add(new ArrangementBeer
        {
            Id = Guid.CreateVersion7(),
            ArrangementId = request.ArrangementId,
            BeerId = request.BeerId,
            NameSnapshot = string.Empty,
            BreweryNameSnapshot = string.Empty,
            BeerStyleSnapshot = string.Empty,
            BeerTypeSnapshot = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        });

        arrangement.RowVersion++;
        arrangement.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Arrangement was modified concurrently. Please reload and retry.");
        }

        return arrangement;
    }
}
