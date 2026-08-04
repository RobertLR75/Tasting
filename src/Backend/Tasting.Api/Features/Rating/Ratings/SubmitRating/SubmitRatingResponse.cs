namespace Tasting.Api.Features.Rating.Ratings.SubmitRating;

public class SubmitRatingResponse
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
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
