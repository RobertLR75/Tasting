using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;

namespace Tasting.Api.IntegrationTests.Arrangement;

public sealed class ArrangementConcurrencyTests(ArrangementApiFactory factory)
    : IClassFixture<ArrangementApiFactory>
{
    [Fact]
    public async Task SeparateWrites_FirstWins_LoserConflicts_AndFreshWriteSucceeds()
    {
        var arrangementId = Guid.NewGuid();
        await factory.SeedArrangementAsync(db => db.Arrangements.Add(new ArrangementRecord
        {
            Id = arrangementId,
            Name = "Original",
            Status = ArrangementStatus.Created,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        }));

        using var winnerScope = factory.Services.CreateScope();
        using var loserScope = factory.Services.CreateScope();
        var winnerDb = winnerScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var loserDb = loserScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();

        var winner = await winnerDb.Arrangements.SingleAsync(item => item.Id == arrangementId);
        var loser = await loserDb.Arrangements.SingleAsync(item => item.Id == arrangementId);

        winner.Name = "Winner";
        winner.RowVersion++;
        loser.Name = "Loser";
        loser.RowVersion++;

        await winnerDb.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => loserDb.SaveChangesAsync());

        using var freshScope = factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var fresh = await freshDb.Arrangements.SingleAsync(item => item.Id == arrangementId);
        Assert.Equal("Winner", fresh.Name);

        fresh.Name = "Fresh retry";
        fresh.RowVersion++;
        await freshDb.SaveChangesAsync();
        Assert.Equal("Fresh retry", (await freshDb.Arrangements.SingleAsync(item => item.Id == arrangementId)).Name);
    }

    [Theory]
    [InlineData(MembershipMutation.AddBeer)]
    [InlineData(MembershipMutation.RemoveBeer)]
    [InlineData(MembershipMutation.AddParticipant)]
    [InlineData(MembershipMutation.RemoveParticipant)]
    public async Task MembershipWrite_ConflictsWithStatusWrite_AndFreshMembershipWriteSucceeds(MembershipMutation mutation)
    {
        var arrangementId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await factory.SeedArrangementAsync(db =>
        {
            var arrangement = NewArrangement(arrangementId);
            arrangement.Beers.Add(NewBeer(arrangementId, memberId));
            arrangement.Participants.Add(NewParticipant(arrangementId, memberId));
            db.Arrangements.Add(arrangement);
        });

        using var membershipScope = factory.Services.CreateScope();
        using var statusScope = factory.Services.CreateScope();
        var membershipDb = membershipScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var statusDb = statusScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var membership = await membershipDb.Arrangements
            .Include(item => item.Beers)
            .Include(item => item.Participants)
            .SingleAsync(item => item.Id == arrangementId);
        var status = await statusDb.Arrangements.SingleAsync(item => item.Id == arrangementId);

        ApplyMembershipMutation(membership, mutation, memberId);
        membership.RowVersion++;
        status.Status = ArrangementStatus.Active;
        status.RowVersion++;

        await statusDb.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => membershipDb.SaveChangesAsync());

        using var freshScope = factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        var fresh = await freshDb.Arrangements
            .Include(item => item.Beers)
            .Include(item => item.Participants)
            .SingleAsync(item => item.Id == arrangementId);
        Assert.Equal(ArrangementStatus.Active, fresh.Status);
        Assert.Single(fresh.Beers);
        Assert.Single(fresh.Participants);

        fresh.Status = ArrangementStatus.Created;
        ApplyMembershipMutation(fresh, mutation, memberId);
        fresh.RowVersion++;
        await freshDb.SaveChangesAsync();

        var isAdd = mutation is MembershipMutation.AddBeer or MembershipMutation.AddParticipant;
        var isBeer = mutation is MembershipMutation.AddBeer or MembershipMutation.RemoveBeer;
        Assert.Equal(isBeer ? (isAdd ? 2 : 0) : 1, fresh.Beers.Count);
        Assert.Equal(isBeer ? 1 : (isAdd ? 2 : 0), fresh.Participants.Count);
        Assert.Equal(fresh.Beers.Count, fresh.Beers.Select(item => item.BeerId).Distinct().Count());
        Assert.Equal(fresh.Participants.Count, fresh.Participants.Select(item => item.UserId).Distinct().Count());
    }

    private static ArrangementRecord NewArrangement(Guid id) => new()
    {
        Id = id,
        Name = "Membership concurrency",
        Status = ArrangementStatus.Created,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ArrangementBeer NewBeer(Guid arrangementId, Guid beerId) => new()
    {
        Id = Guid.NewGuid(),
        ArrangementId = arrangementId,
        BeerId = beerId,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static ArrangementParticipant NewParticipant(Guid arrangementId, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        ArrangementId = arrangementId,
        UserId = userId,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static void ApplyMembershipMutation(ArrangementRecord arrangement, MembershipMutation mutation, Guid memberId)
    {
        switch (mutation)
        {
            case MembershipMutation.AddBeer:
                arrangement.Beers.Add(NewBeer(arrangement.Id, Guid.NewGuid()));
                break;
            case MembershipMutation.RemoveBeer:
                arrangement.Beers.Remove(arrangement.Beers.Single(item => item.BeerId == memberId));
                break;
            case MembershipMutation.AddParticipant:
                arrangement.Participants.Add(NewParticipant(arrangement.Id, Guid.NewGuid()));
                break;
            case MembershipMutation.RemoveParticipant:
                arrangement.Participants.Remove(arrangement.Participants.Single(item => item.UserId == memberId));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }
    }

    public enum MembershipMutation { AddBeer, RemoveBeer, AddParticipant, RemoveParticipant }
}
