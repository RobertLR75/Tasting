namespace Tasting.Api.Features.Arrangement.Beers.RemoveBeer;

public sealed class RemoveBeerRequest
{
    public Guid ArrangementId { get; set; }
    public Guid BeerId { get; set; }
}
