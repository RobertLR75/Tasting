using Microsoft.EntityFrameworkCore;
using Tasting.Api.Contracts;
using ArrangementDomainStatus = Tasting.Api.Features.Arrangement.Domain.ArrangementStatus;

namespace Tasting.Api.Infrastructure.Arrangement;

public sealed class ArrangementService(ArrangementDbContext dbContext) : IArrangementService
{
    public async Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct = default)
    {
        var status = await dbContext.Arrangements
            .Where(a => a.Id == arrangementId)
            .Select(a => (ArrangementDomainStatus?)a.Status)
            .FirstOrDefaultAsync(ct);

        if (status is null)
        {
            throw new SharedLibrary.Services.Exceptions.ServiceNotFoundException(
                $"Arrangement '{arrangementId}' was not found.");
        }

        return (ArrangementStatus)status.Value;
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

    public async Task<string?> GetBeerNameSnapshotAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default)
    {
        return await dbContext.Beers
            .Where(b => b.ArrangementId == arrangementId && b.BeerId == beerId)
            .Select(b => b.NameSnapshot)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetParticipantNameSnapshotAsync(
        Guid arrangementId,
        Guid participantId,
        CancellationToken ct = default)
    {
        return await dbContext.Participants
            .Where(p => p.ArrangementId == arrangementId && p.UserId == participantId)
            .Select(p => $"{p.FirstNameSnapshot} {p.LastNameSnapshot}")
            .FirstOrDefaultAsync(ct);
    }
}
