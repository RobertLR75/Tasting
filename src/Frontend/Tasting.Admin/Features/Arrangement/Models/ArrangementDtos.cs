namespace Tasting.Admin.Features.Arrangement.Models;

public record ArrangementDto(
    int Id,
    string Name,
    DateTime Date,
    string? Description,
    string Status,
    DateTime CreatedAt
);

public record AddArrangementRequest(
    string Name,
    DateTime Date,
    string? Description
);

public record UpdateArrangementRequest(
    string Name,
    DateTime Date,
    string? Description
);

public record ChangeArrangementStatusRequest(
    string NewStatus
);

public record AddBeerToArrangementRequest(
    int BeerId
);

public record RemoveBeerFromArrangementRequest(
    int BeerId
);

public record AddParticipantToArrangementRequest(
    int UserId
);

public record RemoveParticipantFromArrangementRequest(
    int UserId
);

public record ListArrangementsResponse(
    IEnumerable<ArrangementDto> Arrangements,
    int Total
);

public record ArrangementDetailsDto(
    int Id,
    string Name,
    DateTime Date,
    string? Description,
    string Status,
    IEnumerable<BeerInArrangementDto> Beers,
    IEnumerable<ParticipantInArrangementDto> Participants,
    DateTime CreatedAt
);

public record BeerInArrangementDto(
    int BeerId,
    string BeerName,
    string BreweryName
);

public record ParticipantInArrangementDto(
    int UserId,
    string FirstName,
    string LastName,
    string Email
);
