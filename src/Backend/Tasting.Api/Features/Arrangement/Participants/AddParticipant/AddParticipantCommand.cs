using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Participants.AddParticipant;

public sealed record AddParticipantCommand(
    Guid ArrangementId,
    Guid UserId,
    uint RowVersion) : IRequest<Domain.Arrangement>;
