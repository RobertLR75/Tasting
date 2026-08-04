using SharedLibrary.Interfaces;

namespace Tasting.Api.Infrastructure.Rating.Entities;

public class ResultParticipant : IEntityId
{
    public Guid Id { get; set; }
    public Guid ResultId { get; set; }
    public Guid ParticipantId { get; set; }
    public string ParticipantNameSnapshot { get; set; } = string.Empty;
    public decimal Rating { get; set; }
}
