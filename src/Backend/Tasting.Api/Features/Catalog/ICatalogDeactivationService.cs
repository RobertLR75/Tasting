using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog;

public interface ICatalogDeactivationService
{
    Task SaveDeactivationAsync(
        Brewery brewery,
        IReadOnlyCollection<Beer> beers,
        CancellationToken cancellationToken = default);
}
