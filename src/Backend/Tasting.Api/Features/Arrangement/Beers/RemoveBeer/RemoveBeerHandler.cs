using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Beers.RemoveBeer;

public sealed class RemoveBeerHandler(ArrangementDbContext dbContext)
    : IRequestHandler<RemoveBeerCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        RemoveBeerCommand request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .Include(a => a.Beers)
            .FirstOrDefaultAsync(a => a.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        if (arrangement.Status != ArrangementStatus.Created)
        {
            throw new ConflictException(
                $"Beers can only be removed when arrangement is in 'Created' status. Current status: '{arrangement.Status}'.");
        }
        var beer = arrangement.Beers
            .FirstOrDefault(b => b.BeerId == request.BeerId)
            ?? throw new ServiceNotFoundException($"Beer '{request.BeerId}' was not found in arrangement '{request.ArrangementId}'.");

        arrangement.Beers.Remove(beer);
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

        return arrangement.ToDomain();
    }
}
