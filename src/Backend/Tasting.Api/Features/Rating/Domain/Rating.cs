using SharedLibrary.Interfaces;

namespace Tasting.Api.Features.Rating.Domain;

public sealed class Rating : IEntity
{
    public Guid Id { get; set; }
    public Guid ArrangementId { get; init; }
    public Guid ParticipantId { get; init; }
    public Guid BeerId { get; init; }
    public decimal Visibility { get; init; }
    public decimal Smell { get; init; }
    public decimal Taste { get; init; }
    public decimal Toast { get; init; }
    public decimal TotalRating { get; init; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
