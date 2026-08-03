namespace Tasting.Api.Features.Arrangement.Participants.AddParticipant;

public sealed record AddParticipantRequest(Guid UserId, uint RowVersion);
