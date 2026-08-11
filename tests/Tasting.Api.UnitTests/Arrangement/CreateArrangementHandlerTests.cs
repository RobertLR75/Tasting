using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Xunit;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class CreateArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesArrangementWithCreatedStatus()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateArrangementHandler(dbContext);

        var result = await handler.HandleAsync(
            new CreateArrangementCommand("Summer Tasting", "A lovely summer event"),
            CancellationToken.None);

        Assert.Equal("Summer Tasting", result.Name);
        Assert.Equal("A lovely summer event", result.Description);
        Assert.Equal(ArrangementStatus.Created, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task HandleAsync_TrimsName()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateArrangementHandler(dbContext);

        var result = await handler.HandleAsync(
            new CreateArrangementCommand("  Padded Name  ", null),
            CancellationToken.None);

        Assert.Equal("Padded Name", result.Name);
    }

    private static ArrangementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arrangement-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }
}
