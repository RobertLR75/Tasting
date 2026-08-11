using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Infrastructure.Arrangement;

public sealed class ArrangementRecord
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

    public Tasting.Api.Features.Arrangement.Domain.Arrangement ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Status = Status,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt,
        Participants = Participants,
        Beers = Beers
    };

    public static ArrangementRecord FromDomain(Tasting.Api.Features.Arrangement.Domain.Arrangement arrangement) => new()
    {
        Id = arrangement.Id,
        Name = arrangement.Name,
        Description = arrangement.Description,
        Status = arrangement.Status,
        CreatedAt = arrangement.CreatedAt,
        UpdatedAt = arrangement.UpdatedAt,
        Participants = arrangement.Participants,
        Beers = arrangement.Beers
    };
}
