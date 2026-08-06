using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class UpdateArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesNameAndDescription_WhenCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new UpdateArrangementHandler(db);
        var result = await handler.HandleAsync(
            new UpdateArrangementCommand(arrangement.Id, "New Name", "New Desc", 0u));

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Desc", result.Description);
        Assert.Equal(1u, result.RowVersion);
    }

    [Fact]
    public async Task HandleAsync_Updates_WhenRequestRowVersionMatchesCurrentValue()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created, rowVersion: 3u);

        var handler = new UpdateArrangementHandler(db);
        var result = await handler.HandleAsync(
            new UpdateArrangementCommand(arrangement.Id, "Fresh Name", null, 3u));

        Assert.Equal("Fresh Name", result.Name);
        Assert.Equal(4u, result.RowVersion);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenNotCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new UpdateArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new UpdateArrangementCommand(arrangement.Id, "X", null, 0u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenRowVersionMismatch()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new UpdateArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new UpdateArrangementCommand(arrangement.Id, "X", null, 99u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new UpdateArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new UpdateArrangementCommand(Guid.NewGuid(), "X", null, 0u)));
    }

    private static async Task<ArrangementEntity> SeedAsync(
        ArrangementDbContext db,
        ArrangementStatus status,
        uint rowVersion = 0)
    {
        var a = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            Status = status,
            RowVersion = rowVersion,
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
