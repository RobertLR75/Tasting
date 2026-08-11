using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Beers.AddBeer;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class AddBeerHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsBeer_WhenArrangementIsCreated()
    {
        await using var db = CreateArrangementDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);
        var beer = await SeedBeerAsync(catalogDb);

        var handler = new AddBeerHandler(db, catalogDb);

        var result = await handler.HandleAsync(
            new AddBeerCommand(arrangement.Id, beer.Id),
            CancellationToken.None);

        Assert.Single(result.Beers);
        Assert.Equal(beer.Id, result.Beers[0].BeerId);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenArrangementNotCreated()
    {
        await using var db = CreateArrangementDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Started);
        var beer = await SeedBeerAsync(catalogDb);

        var handler = new AddBeerHandler(db, catalogDb);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new AddBeerCommand(arrangement.Id, beer.Id),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenDuplicateBeer()
    {
        await using var db = CreateArrangementDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);
        var beer = await SeedBeerAsync(catalogDb);

        var handler = new AddBeerHandler(db, catalogDb);
        await handler.HandleAsync(
            new AddBeerCommand(arrangement.Id, beer.Id),
            CancellationToken.None);

        var updated = await db.Arrangements.FindAsync(arrangement.Id);
        Assert.NotNull(updated);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new AddBeerCommand(arrangement.Id, beer.Id),
            CancellationToken.None));
    }


    private static async Task<ArrangementEntity> SeedArrangementAsync(
        ArrangementDbContext db, ArrangementStatus status)
    {
        var arrangement = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();
        return arrangement;
    }

    private static async Task<Beer> SeedBeerAsync(CatalogDbContext catalogDb)
    {
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Test Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer
        {
            Id = Guid.NewGuid(),
            BreweryId = brewery.Id,
            BeerStyleId = style.Id,
            BeerTypeId = type.Id,
            Name = "Test Beer",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        catalogDb.AddRange(style, type, brewery, beer);
        await catalogDb.SaveChangesAsync();
        return beer;
    }

    private static ArrangementDbContext CreateArrangementDbContext()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arrangement-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }

    private static CatalogDbContext CreateCatalogDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid()}")
            .Options;
        return new CatalogDbContext(options);
    }
}
