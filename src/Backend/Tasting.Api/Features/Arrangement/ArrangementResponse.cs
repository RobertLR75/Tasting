using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement;

public sealed record ArrangementResponse(
    Guid Id,
    string Name,
    string? Description,
    ArrangementStatus Status,
    uint RowVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
