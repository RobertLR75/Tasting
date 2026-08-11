using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class ListArrangementsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAll_WhenNoFilter()
    {
        await using var db = CreateDb();
        await SeedAsync(db, ArrangementStatus.Created);
        await SeedAsync(db, ArrangementStatus.Started);

        var handler = new ListArrangementsHandler(db);
        var result = await handler.HandleAsync(new ListArrangementsQuery(null));

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersOnStatus()
    {
        await using var db = CreateDb();
        await SeedAsync(db, ArrangementStatus.Created);
        await SeedAsync(db, ArrangementStatus.Started);

        var handler = new ListArrangementsHandler(db);
        var result = await handler.HandleAsync(new ListArrangementsQuery(ArrangementStatus.Created));

        Assert.Single(result.Items);
        Assert.All(result.Items, a => Assert.Equal(ArrangementStatus.Created, a.Status));
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoneMatch()
    {
        await using var db = CreateDb();
        var handler = new ListArrangementsHandler(db);
        var result = await handler.HandleAsync(new ListArrangementsQuery(null));

        Assert.Empty(result.Items);
    }

    private static async Task SeedAsync(ArrangementDbContext db, ArrangementStatus status)
    {
        db.Arrangements.Add(new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static ArrangementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arr-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
