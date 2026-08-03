using SharedLibrary.Interfaces;

namespace Tasting.Api.Infrastructure.Rating.Entities;

public class Result : IEntity
{
    public Guid Id { get; set; }
    public Guid ArrangementId { get; set; }
    public Guid BeerId { get; set; }
    public string BeerNameSnapshot { get; set; } = string.Empty;
    public decimal TotalRating { get; set; }
    public int RatingCount { get; set; }
    public decimal StandardDeviation { get; set; }
    public int Rank { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<ResultParticipant> Participants { get; set; } = [];
}
