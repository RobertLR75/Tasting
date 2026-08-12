using Microsoft.EntityFrameworkCore;
using Tasting.Api.Features.Catalog.Beers.ListBeers;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;

namespace Tasting.Api.UnitTests.Catalog;

public sealed class ListBeersHandlerTests
{
    [Fact]
    public async Task HandleAsync_FiltersInactiveBeersByDefault()
    {
        await using var dbContext = CreateDbContext();
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "Stout", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Dark", CreatedAt = DateTimeOffset.UtcNow };

        dbContext.AddRange(
            brewery,
            style,
            type,
            new Beer
            {
                Id = Guid.NewGuid(),
                BreweryId = brewery.Id,
                BeerStyleId = style.Id,
                BeerTypeId = type.Id,
                Name = "Active Beer",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Beer
            {
                Id = Guid.NewGuid(),
                BreweryId = brewery.Id,
                BeerStyleId = style.Id,
                BeerTypeId = type.Id,
                Name = "Inactive Beer",
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var sut = new ListBeersHandler(new CatalogTestPersistence(dbContext).Beers);

        var result = await sut.HandleAsync(new ListBeersQuery(false), CancellationToken.None);

        Assert.Single(result.Beers);
        Assert.Equal("Active Beer", result.Beers.Single().Name);
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid()}")
            .Options;

        return new CatalogDbContext(options);
    }
}
