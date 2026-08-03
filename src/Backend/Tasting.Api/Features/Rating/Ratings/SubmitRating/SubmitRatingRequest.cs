namespace Tasting.Api.Features.Rating.Ratings.SubmitRating;

public class SubmitRatingRequest
{
    public Guid ArrangementId { get; set; }
    public Guid BeerId { get; set; }
    public decimal Visibility { get; set; }
    public decimal Smell { get; set; }
    public decimal Taste { get; set; }
    public decimal Toast { get; set; }
}
