namespace Tasting.Api.Features.Rating.Results.GetResults;

public class GetResultsResponse
{
    public List<GetResultItem> Results { get; set; } = [];
}

public class GetResultItem
{
    public int Rank { get; set; }
    public Guid BeerId { get; set; }
    public string BeerNameSnapshot { get; set; } = string.Empty;
    public decimal TotalRating { get; set; }
    public int RatingCount { get; set; }
    public decimal StandardDeviation { get; set; }
    public List<GetResultParticipantItem> Participants { get; set; } = [];
}

public class GetResultParticipantItem
{
    public Guid ParticipantId { get; set; }
    public string ParticipantNameSnapshot { get; set; } = string.Empty;
    public decimal Rating { get; set; }
}
