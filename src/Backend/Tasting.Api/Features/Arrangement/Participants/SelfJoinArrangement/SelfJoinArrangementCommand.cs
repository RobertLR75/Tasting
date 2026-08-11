using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Participants.SelfJoinArrangement;

public sealed record SelfJoinArrangementCommand(Guid ArrangementId, Guid UserId)
    : IRequest<SelfJoinArrangementResponse>;

public sealed record SelfJoinArrangementResponse(Guid Id, string Name, Domain.ArrangementStatus Status);
