using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Interfaces;
using Tasting.Api.Features.Catalog;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Tasting.Api.IntegrationTests.Catalog;

public sealed class CatalogProviderContractTests(TastingApiFactory factory)
    : IClassFixture<TastingApiFactory>
{
    [Fact]
    public async Task Providers_MapAndProjectTheSameExistingCatalogData()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = $"Contract Brewery {suffix}", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = $"Contract Style {suffix}", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = $"Contract Type {suffix}", CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer
        {
            Id = Guid.NewGuid(),
            BreweryId = brewery.Id,
            BeerStyleId = style.Id,
            BeerTypeId = type.Id,
            Name = $"Contract Beer {suffix}",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await factory.SeedAsync(db => db.AddRange(brewery, style, type, beer));

        await using var entityFramework = CreateProvider("EntityFramework");
        await using var dapper = CreateProvider("Dapper");
        var efBeers = entityFramework.GetRequiredService<IPersistenceService<Beer>>();
        var dapperBeers = dapper.GetRequiredService<IPersistenceService<Beer>>();

        var efProjection = await efBeers.SearchAsync(new BeerCatalogProjectionSpecification());
        var dapperProjection = await dapperBeers.SearchAsync(new BeerCatalogProjectionSpecification());
        Assert.Equal(
            efProjection.Single(x => x.Id == beer.Id),
            dapperProjection.Single(x => x.Id == beer.Id));

        var efJoined = (await efBeers.SearchAsync(new BeersWithCatalogSpecification(true))).Single(x => x.Id == beer.Id);
        var dapperJoined = (await dapperBeers.SearchAsync(new BeersWithCatalogSpecification(true))).Single(x => x.Id == beer.Id);
        Assert.Equal(efJoined.Name, dapperJoined.Name);
        Assert.Equal(efJoined.Brewery.Name, dapperJoined.Brewery.Name);
        Assert.Equal(efJoined.BeerStyle.Name, dapperJoined.BeerStyle.Name);
        Assert.Equal(efJoined.BeerType.Name, dapperJoined.BeerType.Name);
    }

    private ServiceProvider CreateProvider(string provider)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TastingDb"] = factory.ConnectionString,
                ["Persistence:Provider"] = provider
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatalog(configuration);
        return services.BuildServiceProvider();
    }
}
