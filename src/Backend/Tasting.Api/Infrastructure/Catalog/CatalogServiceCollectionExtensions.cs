using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers.CreateBeer;
using Tasting.Api.Features.Catalog.Beers.ListBeers;
using Tasting.Api.Features.Catalog.Breweries.CreateBrewery;
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

        return services;
    }
}
