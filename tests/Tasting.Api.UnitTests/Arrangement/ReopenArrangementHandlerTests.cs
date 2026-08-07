using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.ReopenArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class ReopenArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_TransitionsToCreated_WhenCanceled()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Canceled, rowVersion: 2u, withMembership: true);

        var handler = new ReopenArrangementHandler(db);
        var result = await handler.HandleAsync(new ReopenArrangementCommand(arrangement.Id, 2u));

        Assert.Equal(ArrangementStatus.Created, result.Status);
        Assert.Equal(3u, result.RowVersion);
        Assert.NotNull(result.UpdatedAt);
        Assert.Single(result.Beers);
        Assert.Single(result.Participants);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenRowVersionMismatch()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Canceled, rowVersion: 2u);

        var handler = new ReopenArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new ReopenArrangementCommand(arrangement.Id, 1u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new ReopenArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new ReopenArrangementCommand(Guid.NewGuid(), 0u)));
    }

    [Theory]
    [InlineData(ArrangementStatus.Created)]
    [InlineData(ArrangementStatus.Active)]
    [InlineData(ArrangementStatus.Started)]
    [InlineData(ArrangementStatus.Completed)]
    public async Task HandleAsync_ThrowsConflict_WhenSourceStatusIsInvalid(ArrangementStatus status)
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, status);

        var handler = new ReopenArrangementHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new ReopenArrangementCommand(arrangement.Id, 0u)));
    }

    private static async Task<ArrangementEntity> SeedAsync(
        ArrangementDbContext db,
        ArrangementStatus status,
        uint rowVersion = 0,
        bool withMembership = false)
    {
        var arrangement = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = rowVersion,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (withMembership)
        {
            arrangement.Beers.Add(new ArrangementBeer
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangement.Id,
                BeerId = Guid.NewGuid(),
                NameSnapshot = "Test Beer",
                CreatedAt = DateTimeOffset.UtcNow
            });
            arrangement.Participants.Add(new ArrangementParticipant
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangement.Id,
                UserId = Guid.NewGuid(),
                FirstNameSnapshot = "Test",
                LastNameSnapshot = "User",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();
        return arrangement;
    }

    private static ArrangementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arr-reopen-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
