namespace Tasting.Admin.Features.Arrangement.Models;

public enum ArrangementStatus
{
    Created = 0,
    Active = 1,
    Started = 2,
    Canceled = 3,
    Completed = 4
}

public record ArrangementBeerItem(Guid Id, Guid BeerId, string BeerName);

public record ArrangementDto(
    Guid Id,
    string Name,
    string? Description,
    ArrangementStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<ArrangementBeerItem> Beers,
    IEnumerable<ArrangementParticipantDto> Participants
);

public record CreateArrangementRequest(
    string Name,
    string? Description = null
);

public record UpdateArrangementRequest(
    string Name,
    string? Description
);

public record ArrangementLifecycleRequest;

public record ListArrangementsResponse(
    IEnumerable<ArrangementDto> Items
);

public record ArrangementBeerDto(
    Guid Id,
    Guid ArrangementId,
    Guid BeerId,
    string BeerName
);

public record AddBeerToArrangementRequest(
    Guid BeerId
);

public record ArrangementParticipantDto(
    Guid Id,
    Guid ArrangementId,
    Guid UserId,
    string UserName
);

public record AddParticipantToArrangementRequest(
    Guid UserId
);
