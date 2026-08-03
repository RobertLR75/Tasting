using Tasting.Api.Contracts;

namespace Tasting.Api.Infrastructure.Rating;

/// <summary>
/// Stub implementation of IArrangementService used until the Arrangement track is integrated.
/// Replace with a real implementation backed by ArrangementDbContext (or a service client).
/// </summary>
public class StubArrangementService : IArrangementService
{
    public Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct = default)
        => Task.FromResult(ArrangementStatus.Started);

    public Task<bool> IsParticipantAsync(Guid arrangementId, Guid userId, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<bool> IsBeerInArrangementAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<string?> GetBeerNameSnapshotAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default)
        => Task.FromResult<string?>("Unknown Beer");

    public Task<string?> GetParticipantNameSnapshotAsync(Guid arrangementId, Guid participantId, CancellationToken ct = default)
        => Task.FromResult<string?>("Unknown Participant");
}
