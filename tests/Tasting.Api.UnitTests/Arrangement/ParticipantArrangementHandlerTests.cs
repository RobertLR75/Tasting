using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Arrangement.Participants.ListVisibleArrangements;
using Tasting.Api.Features.Arrangement.Participants.GetParticipantArrangement;
using Tasting.Api.Features.Arrangement.Participants.SelfJoinArrangement;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Identity;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class ParticipantArrangementHandlerTests
{
    [Fact]
    public async Task GetParticipantArrangement_HidesBeers_WhileWaitingForStart()
    {
        await using var db = CreateArrangementDbContext();
        var userId = Guid.NewGuid();
        var arrangement = CreateArrangement("Waiting", ArrangementStatus.Active);
        arrangement.Participants.Add(new ArrangementParticipant { Id = Guid.NewGuid(), ArrangementId = arrangement.Id, UserId = userId });
        arrangement.Beers.Add(new ArrangementBeer
        {
            Id = Guid.NewGuid(), ArrangementId = arrangement.Id, BeerId = Guid.NewGuid(), NameSnapshot = "Secret beer"
        });
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();

        var result = await new GetParticipantArrangementHandler(db)
            .HandleAsync(new GetParticipantArrangementQuery(arrangement.Id, userId));

        Assert.Equal(ArrangementStatus.Active, result.Status);
        Assert.Empty(result.Beers);
    }

    [Fact]
    public async Task GetParticipantArrangement_RejectsUsersWhoHaveNotJoined()
    {
        await using var db = CreateArrangementDbContext();
        var arrangement = CreateArrangement("Private", ArrangementStatus.Active);
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => new GetParticipantArrangementHandler(db)
            .HandleAsync(new GetParticipantArrangementQuery(arrangement.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task GetParticipantArrangement_RevealsBeerSnapshots_WhenStarted()
    {
        await using var db = CreateArrangementDbContext();
        var userId = Guid.NewGuid();
        var arrangement = CreateArrangement("Started", ArrangementStatus.Started);
        arrangement.Participants.Add(new ArrangementParticipant { Id = Guid.NewGuid(), ArrangementId = arrangement.Id, UserId = userId });
        arrangement.Beers.Add(new ArrangementBeer
        {
            Id = Guid.NewGuid(), ArrangementId = arrangement.Id, BeerId = Guid.NewGuid(), NameSnapshot = "Revealed beer"
        });
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();

        var result = await new GetParticipantArrangementHandler(db)
            .HandleAsync(new GetParticipantArrangementQuery(arrangement.Id, userId));

        Assert.Equal("Revealed beer", Assert.Single(result.Beers).Name);
    }

    [Fact]
    public async Task GetParticipantArrangement_ReturnsNotFound_WhenArrangementDoesNotExist()
    {
        await using var db = CreateArrangementDbContext();

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => new GetParticipantArrangementHandler(db)
            .HandleAsync(new GetParticipantArrangementQuery(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task GetParticipantArrangement_RejectsCanceledArrangement()
    {
        await using var db = CreateArrangementDbContext();
        var userId = Guid.NewGuid();
        var arrangement = CreateArrangement("Canceled", ArrangementStatus.Canceled);
        arrangement.Participants.Add(new ArrangementParticipant { Id = Guid.NewGuid(), ArrangementId = arrangement.Id, UserId = userId });
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => new GetParticipantArrangementHandler(db)
            .HandleAsync(new GetParticipantArrangementQuery(arrangement.Id, userId)));
    }

    [Fact]
    public async Task ListVisibleArrangements_ReturnsOnlyActiveArrangements_AndMembershipState()
    {
        await using var db = CreateArrangementDbContext();
        var userId = Guid.NewGuid();
        var active = CreateArrangement("Active", ArrangementStatus.Active);
        active.Participants.Add(new ArrangementParticipant { Id = Guid.NewGuid(), ArrangementId = active.Id, UserId = userId });
        db.Arrangements.AddRange(active, CreateArrangement("Created", ArrangementStatus.Created), CreateArrangement("Started", ArrangementStatus.Started));
        await db.SaveChangesAsync();

        var result = await new ListVisibleArrangementsHandler(db)
            .HandleAsync(new ListVisibleArrangementsQuery(userId));

        var item = Assert.Single(result.Items);
        Assert.Equal(active.Id, item.Id);
        Assert.True(item.Joined);
    }

    [Fact]
    public async Task SelfJoin_AddsAuthenticatedUser_WhenArrangementIsActive()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        var arrangement = CreateArrangement("Active", ArrangementStatus.Active);
        var user = CreateUser();
        db.Arrangements.Add(arrangement);
        usersDb.Users.Add(user);
        await db.SaveChangesAsync();
        await usersDb.SaveChangesAsync();

        var result = await new SelfJoinArrangementHandler(db, usersDb)
            .HandleAsync(new SelfJoinArrangementCommand(arrangement.Id, user.Id));

        Assert.Equal(ArrangementStatus.Active, result.Status);
        Assert.Contains((await db.Arrangements.Include(item => item.Participants).SingleAsync()).Participants,
            participant => participant.UserId == user.Id);
    }

    [Fact]
    public async Task SelfJoin_RejectsDuplicateMembership()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        var arrangement = CreateArrangement("Active", ArrangementStatus.Active);
        var user = CreateUser();
        arrangement.Participants.Add(new ArrangementParticipant { Id = Guid.NewGuid(), ArrangementId = arrangement.Id, UserId = user.Id });
        db.Arrangements.Add(arrangement);
        usersDb.Users.Add(user);
        await db.SaveChangesAsync();
        await usersDb.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => new SelfJoinArrangementHandler(db, usersDb)
            .HandleAsync(new SelfJoinArrangementCommand(arrangement.Id, user.Id)));
    }

    [Theory]
    [InlineData(ArrangementStatus.Created)]
    [InlineData(ArrangementStatus.Started)]
    [InlineData(ArrangementStatus.Completed)]
    public async Task SelfJoin_RejectsArrangementOutsideActiveStatus(ArrangementStatus status)
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        var arrangement = CreateArrangement("Unavailable", status);
        var user = CreateUser();
        db.Arrangements.Add(arrangement);
        usersDb.Users.Add(user);
        await db.SaveChangesAsync();
        await usersDb.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => new SelfJoinArrangementHandler(db, usersDb)
            .HandleAsync(new SelfJoinArrangementCommand(arrangement.Id, user.Id)));
    }

    private static ArrangementRecord CreateArrangement(string name, ArrangementStatus status) => new()
    {
        Id = Guid.NewGuid(), Name = name, Status = status, CreatedAt = DateTimeOffset.UtcNow
    };

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(), Email = "participant@example.com", EmailNormalized = "participant@example.com",
        FirstName = "Pat", LastName = "Ticipant", IsActive = true, Role = UserRole.User, CreatedAt = DateTimeOffset.UtcNow
    };

    private static ArrangementDbContext CreateArrangementDbContext() => new(
        new DbContextOptionsBuilder<ArrangementDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static UsersDbContext CreateUsersDbContext() => new(
        new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
