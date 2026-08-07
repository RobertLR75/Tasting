namespace Tasting.Admin.Features.Arrangement.Models;

public enum ArrangementStatus
{
    Created = 0,
    Started = 1,
    Completed = 2,
    Canceled = 3
}

public record ArrangementDto(
    Guid Id,
    string Name,
    string? Description,
    ArrangementStatus Status,
    uint RowVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IEnumerable<ArrangementParticipantDto> Participants
);

public record CreateArrangementRequest(
    string Name,
    string? Description = null
);

public record UpdateArrangementRequest(
    string Name,
    string? Description,
    uint RowVersion
);

public record ChangeArrangementStatusRequest(
    ArrangementStatus NewStatus
);

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
    Guid UserId,
    uint RowVersion
);
