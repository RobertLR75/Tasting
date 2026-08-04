namespace Tasting.Api.Contracts;

/// <summary>
/// Cross-context boundary: Rating context queries Arrangement context via this interface.
/// Defined here (Contracts/) until the Arrangement track is integrated.
/// </summary>
public interface IArrangementService
{
    Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct = default);
    Task<bool> IsParticipantAsync(Guid arrangementId, Guid userId, CancellationToken ct = default);
    Task<bool> IsBeerInArrangementAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default);
    Task<string?> GetBeerNameSnapshotAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default);
    Task<string?> GetParticipantNameSnapshotAsync(Guid arrangementId, Guid participantId, CancellationToken ct = default);
}
