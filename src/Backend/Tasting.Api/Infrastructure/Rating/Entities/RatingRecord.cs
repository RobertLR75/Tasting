using DomainRating = Tasting.Api.Features.Rating.Domain.Rating;

namespace Tasting.Api.Infrastructure.Rating.Entities;

public sealed class RatingRecord
{
    public Guid Id { get; set; }
    public Guid ArrangementId { get; set; }
    public Guid ParticipantId { get; set; }
    public Guid BeerId { get; set; }
    public decimal Visibility { get; set; }
    public decimal Smell { get; set; }
    public decimal Taste { get; set; }
    public decimal Toast { get; set; }
    public decimal TotalRating { get; set; }
    public uint RowVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public DomainRating ToDomain() => new()
    {
        Id = Id,
        ArrangementId = ArrangementId,
        ParticipantId = ParticipantId,
        BeerId = BeerId,
        Visibility = Visibility,
        Smell = Smell,
        Taste = Taste,
        Toast = Toast,
        TotalRating = TotalRating,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}
