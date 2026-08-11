using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Arrangement.Participants.AddParticipant;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Identity;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class AddParticipantHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsParticipant_WhenArrangementIsCreated()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);
        var user = await SeedUserAsync(usersDb);

        var handler = new AddParticipantHandler(db, usersDb);

        var result = await handler.HandleAsync(
            new AddParticipantCommand(arrangement.Id, user.Id),
            CancellationToken.None);

        Assert.Single(result.Participants);
        Assert.Equal(user.Id, result.Participants[0].UserId);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenArrangementNotCreated()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Started);
        var user = await SeedUserAsync(usersDb);

        var handler = new AddParticipantHandler(db, usersDb);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new AddParticipantCommand(arrangement.Id, user.Id),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenDuplicateParticipant()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);
        var user = await SeedUserAsync(usersDb);

        var handler = new AddParticipantHandler(db, usersDb);
        await handler.HandleAsync(
            new AddParticipantCommand(arrangement.Id, user.Id),
            CancellationToken.None);

        // Reload to get updated RowVersion
        var updated = await db.Arrangements.FindAsync(arrangement.Id);
        Assert.NotNull(updated);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new AddParticipantCommand(arrangement.Id, user.Id),
            CancellationToken.None));
    }


    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenUserDoesNotExist()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);

        var handler = new AddParticipantHandler(db, usersDb);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => handler.HandleAsync(
            new AddParticipantCommand(arrangement.Id, Guid.NewGuid()),
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

    private static async Task<User> SeedUserAsync(UsersDbContext usersDb)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };
        usersDb.Users.Add(user);
        await usersDb.SaveChangesAsync();
        return user;
    }

    private static ArrangementDbContext CreateArrangementDbContext()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arrangement-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }

    private static UsersDbContext CreateUsersDbContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"users-unit-{Guid.NewGuid()}")
            .Options;
        return new UsersDbContext(options);
    }
}
