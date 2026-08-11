using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Infrastructure.Rating;
using Tasting.Api.Infrastructure.Rating.Entities;

namespace Tasting.Api.IntegrationTests.Rating;

public sealed class RatingPersistenceConcurrencyTests(RatingTestWebFactory factory)
    : IClassFixture<RatingTestWebFactory>
{
    [Fact]
    public async Task SeparateUpdates_FirstWins_LoserConflicts_AndFreshWriteSucceeds()
    {
        var rating = NewRating();
        await SeedAsync(rating);

        using var winnerScope = factory.Services.CreateScope();
        using var loserScope = factory.Services.CreateScope();
        var winnerDb = winnerScope.ServiceProvider.GetRequiredService<RatingDbContext>();
        var loserDb = loserScope.ServiceProvider.GetRequiredService<RatingDbContext>();
        var winner = await winnerDb.Ratings.SingleAsync(item => item.Id == rating.Id);
        var loser = await loserDb.Ratings.SingleAsync(item => item.Id == rating.Id);

        winner.TotalRating = 7m;
        winner.RowVersion++;
        loser.TotalRating = 9m;
        loser.RowVersion++;

        await winnerDb.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => loserDb.SaveChangesAsync());

        using var freshScope = factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<RatingDbContext>();
        var fresh = await freshDb.Ratings.SingleAsync(item => item.Id == rating.Id);
        Assert.Equal(7m, fresh.TotalRating);
        fresh.TotalRating = 6m;
        fresh.RowVersion++;
        await freshDb.SaveChangesAsync();
        Assert.Equal(6m, (await freshDb.Ratings.SingleAsync(item => item.Id == rating.Id)).TotalRating);
    }

    [Fact]
    public async Task SeparateCreates_SameIdentityKey_SecondWriteConflicts()
    {
        var first = NewRating();
        var second = NewRating();
        second.ArrangementId = first.ArrangementId;
        second.ParticipantId = first.ParticipantId;
        second.BeerId = first.BeerId;

        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<RatingDbContext>();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<RatingDbContext>();
        firstDb.Ratings.Add(first);
        secondDb.Ratings.Add(second);

        var outcomes = await Task.WhenAll(
            CaptureWriteAsync(firstDb),
            CaptureWriteAsync(secondDb));

        Assert.Single(outcomes, outcome => outcome is null);
        Assert.Single(outcomes, outcome => outcome is DbUpdateException);
    }

    private async Task SeedAsync(RatingRecord rating)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
        db.Ratings.Add(rating);
        await db.SaveChangesAsync();
    }

    private static async Task<Exception?> CaptureWriteAsync(RatingDbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static RatingRecord NewRating() => new()
    {
        Id = Guid.CreateVersion7(),
        ArrangementId = Guid.NewGuid(),
        ParticipantId = Guid.NewGuid(),
        BeerId = Guid.NewGuid(),
        Visibility = 5m,
        Smell = 5m,
        Taste = 5m,
        Toast = 5m,
        TotalRating = 5m,
        RowVersion = 1,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
