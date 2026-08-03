using SharedLibrary.Services.Interfaces;
using RatingEntity = Tasting.Api.Infrastructure.Rating.Entities.Rating;

namespace Tasting.Api.Features.Rating.Ratings.SubmitRating;

public record SubmitRatingCommand : IRequest<RatingEntity>
{
    public Guid ArrangementId { get; init; }
    public Guid ParticipantId { get; init; }
    public Guid BeerId { get; init; }
    public decimal Visibility { get; init; }
    public decimal Smell { get; init; }
    public decimal Taste { get; init; }
    public decimal Toast { get; init; }
}
