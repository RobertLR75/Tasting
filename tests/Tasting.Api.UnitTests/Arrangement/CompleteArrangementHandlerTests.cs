using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.CompleteArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class CompleteArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_TransitionsToCompleted_WhenStarted()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new CompleteArrangementHandler(db);
        var result = await handler.HandleAsync(new CompleteArrangementCommand(arrangement.Id, 0u));

        Assert.Equal(ArrangementStatus.Completed, result.Status);
        Assert.Equal(1u, result.RowVersion);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new CompleteArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CompleteArrangementCommand(arrangement.Id, 0u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenCanceled()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Canceled);

        var handler = new CompleteArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CompleteArrangementCommand(arrangement.Id, 0u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenRowVersionMismatch()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new CompleteArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CompleteArrangementCommand(arrangement.Id, 99u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new CompleteArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new CompleteArrangementCommand(Guid.NewGuid(), 0u)));
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
