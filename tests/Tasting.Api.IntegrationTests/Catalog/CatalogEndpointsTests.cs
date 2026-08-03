using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Catalog.Beers.ListBeers;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Tasting.Api.IntegrationTests.Catalog;

public sealed class CatalogEndpointsTests : IClassFixture<TastingApiFactory>
{
    private readonly TastingApiFactory _factory;

    public CatalogEndpointsTests(TastingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBrewery_ReturnsForbidden_ForNonAdmin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.PostAsJsonAsync(
            "/api/v1/breweries",
            new { name = "Forbidden Brewery", isActive = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBeer_RejectsInactiveBrewery()
    {
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var breweryId = Guid.NewGuid();
        await SeedAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "IPA", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Ale", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Inactive", IsActive = false, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            "/api/v1/beers",
            new
            {
                breweryId,
                beerStyleId = styleId,
                beerTypeId = typeId,
                name = "Nope",
                isActive = true
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ListBeers_FiltersInactiveByDefault()
    {
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var breweryId = Guid.NewGuid();
        await SeedAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "Stout", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Dark", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
            db.Beers.Add(new Beer
            {
                Id = Guid.NewGuid(),
                BreweryId = breweryId,
                BeerStyleId = styleId,
                BeerTypeId = typeId,
                Name = "Active",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.Beers.Add(new Beer
            {
                Id = Guid.NewGuid(),
                BreweryId = breweryId,
                BeerStyleId = styleId,
                BeerTypeId = typeId,
                Name = "Inactive",
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.GetAsync("/api/v1/beers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ListBeersResponse>();
        Assert.NotNull(payload);
        Assert.Single(payload.Beers);
        Assert.Equal("Active", payload.Beers.Single().Name);
    }

    private async Task SeedAsync(Action<CatalogDbContext> seedAction)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        seedAction(dbContext);
        await dbContext.SaveChangesAsync();
    }
}
