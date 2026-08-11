namespace Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;

public sealed class RemoveParticipantRequest
{
    public Guid ArrangementId { get; set; }
    public Guid UserId { get; set; }
}
