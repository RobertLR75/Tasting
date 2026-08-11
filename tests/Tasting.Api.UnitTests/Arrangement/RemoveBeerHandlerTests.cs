using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Beers.RemoveBeer;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class RemoveBeerHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesBeer_WhenCreated()
    {
        await using var db = CreateDb();
        var beerId = Guid.NewGuid();
        var arrangement = await SeedWithBeerAsync(db, beerId, ArrangementStatus.Created);

        var handler = new RemoveBeerHandler(db);
        var result = await handler.HandleAsync(
            new RemoveBeerCommand(arrangement.Id, beerId));

        Assert.Empty(result.Beers);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenNotCreated()
    {
        await using var db = CreateDb();
        var beerId = Guid.NewGuid();
        var arrangement = await SeedWithBeerAsync(db, beerId, ArrangementStatus.Started);

        var handler = new RemoveBeerHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RemoveBeerCommand(arrangement.Id, beerId)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenBeerNotInArrangement()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new RemoveBeerHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new RemoveBeerCommand(arrangement.Id, Guid.NewGuid())));
    }


    private static async Task<ArrangementEntity> SeedAsync(ArrangementDbContext db, ArrangementStatus status)
    {
        var a = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Arrangements.Add(a);
        await db.SaveChangesAsync();
        return a;
    }

    private static async Task<ArrangementEntity> SeedWithBeerAsync(
        ArrangementDbContext db, Guid beerId, ArrangementStatus status)
    {
        var a = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        a.Beers.Add(new ArrangementBeer
        {
            Id = Guid.NewGuid(),
            ArrangementId = a.Id,
            BeerId = beerId,
            NameSnapshot = string.Empty,
            BreweryNameSnapshot = string.Empty,
            BeerStyleSnapshot = string.Empty,
            BeerTypeSnapshot = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.Arrangements.Add(a);
        await db.SaveChangesAsync();
        return a;
    }

    private static ArrangementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arr-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
