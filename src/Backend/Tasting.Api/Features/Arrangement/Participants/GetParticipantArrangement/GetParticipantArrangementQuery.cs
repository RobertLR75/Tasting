using SharedLibrary.Services.Interfaces;
using System.Text.Json.Serialization;
using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement.Participants.GetParticipantArrangement;

public sealed record GetParticipantArrangementQuery(Guid ArrangementId, Guid UserId)
    : IRequest<ParticipantArrangementResponse>;

public sealed record ParticipantArrangementResponse(
    Guid Id,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ArrangementStatus Status,
    IReadOnlyList<ParticipantBeerResponse> Beers);

public sealed record ParticipantBeerResponse(
    Guid Id,
    string Name,
    string BreweryName,
    string BeerStyle,
    string BeerType);
