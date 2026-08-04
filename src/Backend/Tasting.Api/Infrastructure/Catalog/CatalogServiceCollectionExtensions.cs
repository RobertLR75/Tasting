using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
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
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Infrastructure.Catalog;

public static class CatalogServiceCollectionExtensions
{
    public static IServiceCollection AddCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TastingDb");
        services.AddDbContext<CatalogDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("tasting-catalog");
                return;
            }

            options.UseNpgsql(connectionString);
        });

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

        return services;
    }
}
