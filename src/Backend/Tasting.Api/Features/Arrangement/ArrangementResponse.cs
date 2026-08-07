using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement;

public sealed record ArrangementBeerItem(Guid Id, Guid BeerId, string BeerName);

public sealed record ArrangementParticipantResponse(Guid Id, Guid UserId, string UserName);

public sealed record ArrangementResponse(
    Guid Id,
    string Name,
    string? Description,
    ArrangementStatus Status,
    uint RowVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<ArrangementBeerItem> Beers,
    IReadOnlyList<ArrangementParticipantResponse> Participants);
