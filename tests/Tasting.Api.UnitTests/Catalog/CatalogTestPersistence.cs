using SharedLibrary.Interfaces;
using Tasting.Api.Features.Catalog;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.UnitTests.Catalog;

internal sealed class CatalogTestPersistence
{
    public CatalogTestPersistence(CatalogDbContext context)
    {
        context.ChangeTracker.Clear();
        Breweries = new EfCatalogStorage<Brewery>(context);
        Beers = new EfCatalogStorage<Beer>(context);
        Styles = new EfCatalogStorage<BeerStyle>(context);
        Types = new EfCatalogStorage<BeerType>(context);
        Deactivation = new EfCatalogDeactivationService(context);
    }

    public IPersistenceService<Brewery> Breweries { get; }
    public IPersistenceService<Beer> Beers { get; }
    public IPersistenceService<BeerStyle> Styles { get; }
    public IPersistenceService<BeerType> Types { get; }
    public ICatalogDeactivationService Deactivation { get; }
}
