using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class GetArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsArrangement_WhenFound()
    {
        await using var db = CreateDb();
        var arrangement = await SeedAsync(db, ArrangementStatus.Created);

        var handler = new GetArrangementHandler(db);
        var result = await handler.HandleAsync(new GetArrangementQuery(arrangement.Id));

        Assert.Equal(arrangement.Id, result.Id);
        Assert.Equal(ArrangementStatus.Created, result.Status);
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenMissing()
    {
        await using var db = CreateDb();
        var handler = new GetArrangementHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() =>
            handler.HandleAsync(new GetArrangementQuery(Guid.NewGuid())));
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
