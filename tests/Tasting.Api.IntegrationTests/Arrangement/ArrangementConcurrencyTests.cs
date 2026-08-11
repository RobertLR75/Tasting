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
}
