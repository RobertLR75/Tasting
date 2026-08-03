using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Contracts;

public interface IArrangementService
{
    Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct = default);
    Task<bool> IsParticipantAsync(Guid arrangementId, Guid userId, CancellationToken ct = default);
    Task<bool> IsBeerInArrangementAsync(Guid arrangementId, Guid beerId, CancellationToken ct = default);
}
