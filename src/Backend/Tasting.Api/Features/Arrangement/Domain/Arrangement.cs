using SharedLibrary.Interfaces;

namespace Tasting.Api.Features.Arrangement.Domain;

public sealed class Arrangement : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ArrangementStatus Status { get; set; }
    public uint RowVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<ArrangementParticipant> Participants { get; set; } = [];
    public List<ArrangementBeer> Beers { get; set; } = [];
}
