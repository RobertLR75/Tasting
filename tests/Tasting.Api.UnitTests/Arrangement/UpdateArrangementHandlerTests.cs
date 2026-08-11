using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

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
            new UpdateArrangementCommand(arrangement.Id, "New Name", "New Desc"));

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task HandleAsync_Updates_WhenRequestRowVersionMatchesCurrentValue()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created, rowVersion: 3u);

        var handler = new UpdateArrangementHandler(db);
        var result = await handler.HandleAsync(
            new UpdateArrangementCommand(arrangement.Id, "Fresh Name", null));

        Assert.Equal("Fresh Name", result.Name);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenNotCreated()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Started);

        var handler = new UpdateArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new UpdateArrangementCommand(arrangement.Id, "X", null)));
    }


    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new UpdateArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new UpdateArrangementCommand(Guid.NewGuid(), "X", null)));
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
