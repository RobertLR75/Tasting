using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.ActivateArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class ActivateArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_TransitionsToActive_WhenCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new ActivateArrangementHandler(db);
        var result = await handler.HandleAsync(new ActivateArrangementCommand(arrangement.Id));

        Assert.Equal(ArrangementStatus.Active, result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenWrongStatus()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new ActivateArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new ActivateArrangementCommand(arrangement.Id)));
    }


    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new ActivateArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new ActivateArrangementCommand(Guid.NewGuid())));
    }

    private static async Task<ArrangementEntity> SeedAsync(ArrangementDbContext db, ArrangementStatus status)
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

    private static ArrangementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arr-activate-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
