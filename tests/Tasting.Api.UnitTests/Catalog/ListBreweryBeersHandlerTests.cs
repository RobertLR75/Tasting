using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;

namespace Tasting.Api.UnitTests.Catalog;

public sealed class ListBreweryBeersHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOnlyBeersForBrewery()
    {
        await using var dbContext = CreateDbContext();

        var brewery1 = new Brewery { Id = Guid.NewGuid(), Name = "Mack Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var brewery2 = new Brewery { Id = Guid.NewGuid(), Name = "Other Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "Lager", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Pale", CreatedAt = DateTimeOffset.UtcNow };

        dbContext.AddRange(
            brewery1, brewery2, style, type,
            new Beer { Id = Guid.NewGuid(), BreweryId = brewery1.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Arctic Beer", IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new Beer { Id = Guid.NewGuid(), BreweryId = brewery1.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Mack Pilsner", IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new Beer { Id = Guid.NewGuid(), BreweryId = brewery2.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Other Beer", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync();

        var persistence = new CatalogTestPersistence(dbContext);
        var sut = new ListBreweryBeersHandler(persistence.Breweries, persistence.Beers);

        var result = await sut.HandleAsync(new ListBreweryBeersQuery(brewery1.Id), CancellationToken.None);

        Assert.Equal(2, result.Beers.Count);
        Assert.All(result.Beers, b => Assert.Equal(brewery1.Id, b.BreweryId));
        Assert.All(result.Beers, b => Assert.Equal("Mack Brewery", b.BreweryName));
        Assert.DoesNotContain(result.Beers, b => b.Name == "Other Beer");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenBreweryDoesNotExist()
    {
        await using var dbContext = CreateDbContext();

        var persistence = new CatalogTestPersistence(dbContext);
        var sut = new ListBreweryBeersHandler(persistence.Breweries, persistence.Beers);

        await Assert.ThrowsAsync<ServiceNotFoundException>(
            () => sut.HandleAsync(new ListBreweryBeersQuery(Guid.NewGuid()), CancellationToken.None));
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-brewery-beers-{Guid.NewGuid()}")
            .Options;

        return new CatalogDbContext(options);
    }
}
