using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Beers.CreateBeer;
using Tasting.Api.Features.Catalog.Beers.DeactivateBeer;
using Tasting.Api.Features.Catalog.Beers.GetBeer;
using Tasting.Api.Features.Catalog.Beers.ListBeers;
using Tasting.Api.Features.Catalog.Beers.UpdateBeer;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;
using Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;
using Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;
using Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;
using Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Breweries.CreateBrewery;
using Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;
using Tasting.Api.Features.Catalog.Breweries.GetBrewery;
using Tasting.Api.Features.Catalog.Breweries.ListBreweries;
using Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;
using Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Infrastructure.Catalog;

public static class CatalogServiceCollectionExtensions
{
    public static IServiceCollection AddCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        var persistence = PersistenceConfigurationSelector.Select(configuration);
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(persistence.ConnectionString));

        if (persistence.Provider == PersistenceProvider.EntityFramework)
        {
            services.AddScoped<IPersistenceService<Brewery>, EfCatalogStorage<Brewery>>();
            services.AddScoped<IPersistenceService<Beer>, EfCatalogStorage<Beer>>();
            services.AddScoped<IPersistenceService<BeerStyle>, EfCatalogStorage<BeerStyle>>();
            services.AddScoped<IPersistenceService<BeerType>, EfCatalogStorage<BeerType>>();
            services.AddScoped<ICatalogDeactivationService, EfCatalogDeactivationService>();
        }
        else
        {
            services.AddScoped(_ => new NpgsqlConnection(persistence.ConnectionString));
            services.AddScoped<DbConnection>(provider => provider.GetRequiredService<NpgsqlConnection>());
            services.AddScoped<IPersistenceService<Brewery>, DapperBreweryStorage>();
            services.AddScoped<IPersistenceService<Beer>, DapperBeerStorage>();
            services.AddScoped<IPersistenceService<BeerStyle>, DapperBeerStyleStorage>();
            services.AddScoped<IPersistenceService<BeerType>, DapperBeerTypeStorage>();
            services.AddScoped<ICatalogDeactivationService, DapperCatalogDeactivationService>();
        }

        services.AddScoped<IRequestHandler<CreateBreweryCommand, Brewery>, CreateBreweryHandler>();
        services.AddScoped<IRequestHandler<CreateBeerCommand, Beer>, CreateBeerHandler>();
        services.AddScoped<IRequestHandler<ListBeersQuery, ListBeersResult>, ListBeersHandler>();
        services.AddScoped<IRequestHandler<GetBreweryQuery, BreweryResponse>, GetBreweryHandler>();
        services.AddScoped<IRequestHandler<ListBreweriesQuery, ListBreweriesResponse>, ListBreweriesHandler>();
        services.AddScoped<IRequestHandler<UpdateBreweryCommand, Brewery>, UpdateBreweryHandler>();
        services.AddScoped<IRequestHandler<DeactivateBreweryCommand, Brewery>, DeactivateBreweryHandler>();
        services.AddScoped<IRequestHandler<GetBeerQuery, BeerResponse>, GetBeerHandler>();
        services.AddScoped<IRequestHandler<UpdateBeerCommand, Beer>, UpdateBeerHandler>();
        services.AddScoped<IRequestHandler<DeactivateBeerCommand, Beer>, DeactivateBeerHandler>();
        services.AddScoped<IRequestHandler<CreateBeerStyleCommand, BeerStyle>, CreateBeerStyleHandler>();
        services.AddScoped<IRequestHandler<GetBeerStyleQuery, BeerStyleResponse>, GetBeerStyleHandler>();
        services.AddScoped<IRequestHandler<ListBeerStylesQuery, ListBeerStylesResponse>, ListBeerStylesHandler>();
        services.AddScoped<IRequestHandler<CreateBeerTypeCommand, BeerType>, CreateBeerTypeHandler>();
        services.AddScoped<IRequestHandler<GetBeerTypeQuery, BeerTypeResponse>, GetBeerTypeHandler>();
        services.AddScoped<IRequestHandler<ListBeerTypesQuery, ListBeerTypesResponse>, ListBeerTypesHandler>();
        services.AddScoped<IRequestHandler<ListBreweryBeersQuery, ListBreweryBeersResult>, ListBreweryBeersHandler>();

        return services;
    }
}
