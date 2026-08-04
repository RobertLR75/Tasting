using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class RemoveParticipantHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesParticipant_WhenCreated()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var arrangement = await SeedWithParticipantAsync(db, userId, ArrangementStatus.Created);

        var handler = new RemoveParticipantHandler(db);
        var result = await handler.HandleAsync(
            new RemoveParticipantCommand(arrangement.Id, userId, 0u));

        Assert.Empty(result.Participants);
        Assert.Equal(1u, result.RowVersion);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenNotCreated()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var arrangement = await SeedWithParticipantAsync(db, userId, ArrangementStatus.Started);

        var handler = new RemoveParticipantHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RemoveParticipantCommand(arrangement.Id, userId, 0u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenParticipantNotInArrangement()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new RemoveParticipantHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new RemoveParticipantCommand(arrangement.Id, Guid.NewGuid(), 0u)));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenRowVersionMismatch()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var arrangement = await SeedWithParticipantAsync(db, userId, ArrangementStatus.Created);

        var handler = new RemoveParticipantHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RemoveParticipantCommand(arrangement.Id, userId, 99u)));
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

    private static async Task<ArrangementEntity> SeedWithParticipantAsync(
        ArrangementDbContext db, Guid userId, ArrangementStatus status)
    {
        var a = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        a.Participants.Add(new ArrangementParticipant
        {
            Id = Guid.NewGuid(),
            ArrangementId = a.Id,
            UserId = userId,
            FirstNameSnapshot = string.Empty,
            LastNameSnapshot = string.Empty,
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
