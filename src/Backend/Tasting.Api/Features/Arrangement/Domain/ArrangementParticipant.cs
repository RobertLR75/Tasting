namespace Tasting.Api.Features.Arrangement.Domain;

public sealed class ArrangementParticipant
{
    public Guid Id { get; set; }
    public Guid ArrangementId { get; set; }
    public Guid UserId { get; set; }
    public string FirstNameSnapshot { get; set; } = string.Empty;
    public string LastNameSnapshot { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
