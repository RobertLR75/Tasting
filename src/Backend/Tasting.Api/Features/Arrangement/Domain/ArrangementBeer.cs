namespace Tasting.Api.Features.Arrangement.Domain;

public sealed class ArrangementBeer
{
    public Guid Id { get; set; }
    public Guid ArrangementId { get; set; }
    public Guid BeerId { get; set; }
    public string NameSnapshot { get; set; } = string.Empty;
    public string BreweryNameSnapshot { get; set; } = string.Empty;
    public string BeerStyleSnapshot { get; set; } = string.Empty;
    public string BeerTypeSnapshot { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
