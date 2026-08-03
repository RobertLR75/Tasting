using SharedLibrary.Interfaces;

namespace Tasting.Api.Features.Catalog.Domain;

public sealed class Brewery : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Beer> Beers { get; set; } = [];
}
