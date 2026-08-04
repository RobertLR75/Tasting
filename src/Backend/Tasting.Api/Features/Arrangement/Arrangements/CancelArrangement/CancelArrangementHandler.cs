using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;

public sealed class CancelArrangementHandler(ArrangementDbContext dbContext)
    : IRequestHandler<CancelArrangementCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        CancelArrangementCommand request,
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
                $"Arrangement cannot be cancelled from status '{arrangement.Status}'. Only 'Created' arrangements can be cancelled.");
        }

        if (arrangement.RowVersion != request.RowVersion)
        {
            throw new ConflictException(
                "Arrangement has been modified by another request. Please reload and retry.");
        }

        arrangement.Status = ArrangementStatus.Canceled;
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
