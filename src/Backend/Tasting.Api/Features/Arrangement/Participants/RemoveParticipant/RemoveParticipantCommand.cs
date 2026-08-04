using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;

public sealed record RemoveParticipantCommand(
    Guid ArrangementId,
    Guid UserId,
    uint RowVersion) : IRequest<Domain.Arrangement>;
