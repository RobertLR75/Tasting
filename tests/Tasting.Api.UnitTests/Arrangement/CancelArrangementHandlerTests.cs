using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class CancelArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_TransitionsToCanceled_WhenCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new CancelArrangementHandler(db);
        var result = await handler.HandleAsync(new CancelArrangementCommand(arrangement.Id));

        Assert.Equal(ArrangementStatus.Canceled, result.Status);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenStarted()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new CancelArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CancelArrangementCommand(arrangement.Id)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenCompleted()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Completed);

        var handler = new CancelArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CancelArrangementCommand(arrangement.Id)));
    }


    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new CancelArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new CancelArrangementCommand(Guid.NewGuid())));
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

    private static ArrangementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arr-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
