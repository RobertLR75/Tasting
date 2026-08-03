using Microsoft.EntityFrameworkCore;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Infrastructure.Arrangement;

public sealed class ArrangementService(ArrangementDbContext dbContext) : IArrangementService
{
    public async Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct = default)
    {
        var status = await dbContext.Arrangements
            .Where(a => a.Id == arrangementId)
            .Select(a => (ArrangementStatus?)a.Status)
            .FirstOrDefaultAsync(ct);

        if (status is null)
        {
            throw new SharedLibrary.Services.Exceptions.ServiceNotFoundException(
                $"Arrangement '{arrangementId}' was not found.");
        }

        return status.Value;
    }

    public async Task<bool> IsParticipantAsync(Guid arrangementId, Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Participants
            .AnyAsync(p => p.ArrangementId == arrangementId && p.UserId == userId, ct);
    }

    public async Task<bool> IsBeerInArrangementAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default)
    {
        return await dbContext.Beers
            .AnyAsync(b => b.ArrangementId == arrangementId && b.BeerId == beerId, ct);
    }
}
