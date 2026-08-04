using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Beers.ListBeers;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Breweries.ListBreweries;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Tasting.Api.IntegrationTests.Catalog;

public sealed class CatalogEndpointsTests : IClassFixture<TastingApiFactory>, IAsyncLifetime
{
    private readonly TastingApiFactory _factory;

    public CatalogEndpointsTests(TastingApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureSystemUsersSeededAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetBrewery_ReturnsBrewery_ForAuthenticatedUser()
    {
        var breweryId = Guid.NewGuid();
        await _factory.SeedAsync(db => db.Breweries.Add(new Brewery
        {
            Id = breweryId,
            Name = "Nogne",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        }));

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync($"/api/v1/breweries/{breweryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BreweryResponse>();
        Assert.NotNull(payload);
        Assert.Equal(breweryId, payload.Id);
    }

    [Fact]
    public async Task ListBreweries_FiltersInactiveByDefault()
    {
        var activeName = $"Active Brewery {Guid.NewGuid():N}";
        var inactiveName = $"Inactive Brewery {Guid.NewGuid():N}";
        await _factory.SeedAsync(db =>
        {
            db.Breweries.AddRange(
                new Brewery { Id = Guid.NewGuid(), Name = activeName, IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
                new Brewery { Id = Guid.NewGuid(), Name = inactiveName, IsActive = false, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync("/api/v1/breweries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ListBreweriesResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload.Breweries, x => x.Name == activeName);
        Assert.DoesNotContain(payload.Breweries, x => x.Name == inactiveName);
    }

    [Fact]
    public async Task UpdateBrewery_UpdatesBrewery_ForAdmin()
    {
        var breweryId = Guid.NewGuid();
        await _factory.SeedAsync(db => db.Breweries.Add(new Brewery
        {
            Id = breweryId,
            Name = "Old Brewery",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        }));

        using var client = CreateAuthorizedClient("admin");
        var response = await client.PutAsJsonAsync($"/api/v1/breweries/{breweryId}", new
        {
            id = breweryId,
            name = "New Brewery",
            isActive = false
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var brewery = await db.Breweries.FindAsync(breweryId);
        Assert.NotNull(brewery);
        Assert.Equal("New Brewery", brewery.Name);
        Assert.False(brewery.IsActive);
    }

    [Fact]
    public async Task DeactivateBrewery_DeactivatesAllBeers_ForAdmin()
    {
        var breweryId = Guid.NewGuid();
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "IPA", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Ale", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Cascade", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
            db.Beers.AddRange(
                new Beer { Id = Guid.NewGuid(), BreweryId = breweryId, BeerStyleId = styleId, BeerTypeId = typeId, Name = "One", IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
                new Beer { Id = Guid.NewGuid(), BreweryId = breweryId, BeerStyleId = styleId, BeerTypeId = typeId, Name = "Two", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = CreateAuthorizedClient("admin");
        var response = await client.PatchAsync(
            $"/api/v1/breweries/{breweryId}/deactivate",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.False((await db.Breweries.FindAsync(breweryId))!.IsActive);
        Assert.All(db.Beers.Where(x => x.BreweryId == breweryId).ToList(), beer => Assert.False(beer.IsActive));
    }

    [Fact]
    public async Task GetBeer_ReturnsBeer_ForAuthenticatedUser()
    {
        var beerId = await SeedBeerAsync();

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync($"/api/v1/beers/{beerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeerResponse>();
        Assert.NotNull(payload);
        Assert.Equal(beerId, payload.Id);
    }

    [Fact]
    public async Task UpdateBeer_RejectsDuplicateNameCaseInsensitive()
    {
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var breweryId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "IPA", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Ale", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Dupes", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
            db.Beers.AddRange(
                new Beer { Id = beerId, BreweryId = breweryId, BeerStyleId = styleId, BeerTypeId = typeId, Name = "First", IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
                new Beer { Id = Guid.NewGuid(), BreweryId = breweryId, BeerStyleId = styleId, BeerTypeId = typeId, Name = "Second", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = CreateAuthorizedClient("admin");
        var response = await client.PutAsJsonAsync($"/api/v1/beers/{beerId}", new
        {
            id = beerId,
            breweryId,
            beerStyleId = styleId,
            beerTypeId = typeId,
            name = "SECOND",
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateBeer_DeactivatesBeer_ForAdmin()
    {
        var beerId = await SeedBeerAsync();

        using var client = CreateAuthorizedClient("admin");
        var response = await client.PatchAsync(
            $"/api/v1/beers/{beerId}/deactivate",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        Assert.False((await db.Beers.FindAsync(beerId))!.IsActive);
    }

    [Fact]
    public async Task CreateBeerStyle_CreatesStyle_ForAdmin()
    {
        using var client = CreateAuthorizedClient("admin");
        var response = await client.PostAsJsonAsync("/api/v1/beer-styles", new { name = "Sour" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeerStyleResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Sour", payload.Name);
    }

    [Fact]
    public async Task GetBeerStyle_ReturnsStyle_ForAuthenticatedUser()
    {
        var styleId = Guid.NewGuid();
        await _factory.SeedAsync(db => db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "Porter", CreatedAt = DateTimeOffset.UtcNow }));

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync($"/api/v1/beer-styles/{styleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeerStyleResponse>();
        Assert.NotNull(payload);
        Assert.Equal(styleId, payload.Id);
    }

    [Fact]
    public async Task ListBeerStyles_ReturnsOrderedStyles()
    {
        var stoutName = $"Stout {Guid.NewGuid():N}";
        var amberName = $"Amber {Guid.NewGuid():N}";
        await _factory.SeedAsync(db =>
        {
            db.BeerStyles.AddRange(
                new BeerStyle { Id = Guid.NewGuid(), Name = stoutName, CreatedAt = DateTimeOffset.UtcNow },
                new BeerStyle { Id = Guid.NewGuid(), Name = amberName, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync("/api/v1/beer-styles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ListBeerStylesResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload.BeerStyles, x => x.Name == amberName);
        Assert.Contains(payload.BeerStyles, x => x.Name == stoutName);
    }

    [Fact]
    public async Task CreateBeerType_CreatesType_ForAdmin()
    {
        using var client = CreateAuthorizedClient("admin");
        var response = await client.PostAsJsonAsync("/api/v1/beer-types", new { name = "Lager" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeerTypeResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Lager", payload.Name);
    }

    [Fact]
    public async Task GetBeerType_ReturnsType_ForAuthenticatedUser()
    {
        var typeId = Guid.NewGuid();
        await _factory.SeedAsync(db => db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Saison", CreatedAt = DateTimeOffset.UtcNow }));

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync($"/api/v1/beer-types/{typeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeerTypeResponse>();
        Assert.NotNull(payload);
        Assert.Equal(typeId, payload.Id);
    }

    [Fact]
    public async Task ListBeerTypes_ReturnsOrderedTypes()
    {
        var stoutName = $"Stout {Guid.NewGuid():N}";
        var aleName = $"Ale {Guid.NewGuid():N}";
        await _factory.SeedAsync(db =>
        {
            db.BeerTypes.AddRange(
                new BeerType { Id = Guid.NewGuid(), Name = stoutName, CreatedAt = DateTimeOffset.UtcNow },
                new BeerType { Id = Guid.NewGuid(), Name = aleName, CreatedAt = DateTimeOffset.UtcNow });
        });

        using var client = CreateAuthorizedClient("user");
        var response = await client.GetAsync("/api/v1/beer-types");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ListBeerTypesResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload.BeerTypes, x => x.Name == aleName);
        Assert.Contains(payload.BeerTypes, x => x.Name == stoutName);
    }

    private HttpClient CreateAuthorizedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<Guid> SeedBeerAsync()
    {
        var beerId = Guid.NewGuid();
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var breweryId = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "Style", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Type", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
            db.Beers.Add(new Beer
            {
                Id = beerId,
                BreweryId = breweryId,
                BeerStyleId = styleId,
                BeerTypeId = typeId,
                Name = "Beer",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        return beerId;
    }
}
