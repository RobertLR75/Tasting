using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Rating.Results.GetResults;
using Tasting.Api.Infrastructure.Rating;
using Tasting.Api.Infrastructure.Rating.Entities;

namespace Tasting.Api.UnitTests.Rating;

public class GetResultsHandlerTests : IDisposable
{
    private readonly RatingDbContext _db;
    private readonly GetResultsHandler _handler;

    public GetResultsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<RatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new RatingDbContext(options);
        _handler = new GetResultsHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Result> SeedResultAsync(Guid arrangementId, Guid beerId, decimal totalRating,
        int ratingCount = 1, decimal stdDev = 0m, int rank = 1, string beerName = "Beer")
    {
        var result = new Result
        {
            Id = Guid.NewGuid(),
            ArrangementId = arrangementId,
            BeerId = beerId,
            BeerNameSnapshot = beerName,
            TotalRating = totalRating,
            RatingCount = ratingCount,
            StandardDeviation = stdDev,
            Rank = rank,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        return result;
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoResults()
    {
        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = Guid.NewGuid() });
        Assert.Empty(response.Results);
    }

    [Fact]
    public async Task HandleAsync_ReturnsSingleResult()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        await SeedResultAsync(arrangementId, beerId, 8.5m, beerName: "Pilsner");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangementId });

        Assert.Single(response.Results);
        Assert.Equal("Pilsner", response.Results[0].BeerNameSnapshot);
        Assert.Equal(8.5m, response.Results[0].TotalRating);
    }

    [Fact]
    public async Task HandleAsync_RanksBy_TotalRatingDesc()
    {
        var arrangementId = Guid.NewGuid();
        var beer1 = Guid.NewGuid();
        var beer2 = Guid.NewGuid();

        await SeedResultAsync(arrangementId, beer1, 7.0m, beerName: "Low");
        await SeedResultAsync(arrangementId, beer2, 9.0m, beerName: "High");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangementId });

        Assert.Equal(2, response.Results.Count);
        Assert.Equal(1, response.Results[0].Rank);
        Assert.Equal("High", response.Results[0].BeerNameSnapshot);
        Assert.Equal(2, response.Results[1].Rank);
        Assert.Equal("Low", response.Results[1].BeerNameSnapshot);
    }

    [Fact]
    public async Task HandleAsync_TieBreak_RatingCountDesc()
    {
        var arrangementId = Guid.NewGuid();
        var beer1 = Guid.NewGuid();
        var beer2 = Guid.NewGuid();

        // Same TotalRating, beer2 has more ratings
        await SeedResultAsync(arrangementId, beer1, 8.0m, ratingCount: 1, beerName: "Fewer");
        await SeedResultAsync(arrangementId, beer2, 8.0m, ratingCount: 3, beerName: "More");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangementId });

        Assert.Equal(1, response.Results[0].Rank);
        Assert.Equal("More", response.Results[0].BeerNameSnapshot);
    }

    [Fact]
    public async Task HandleAsync_TieBreak_StandardDeviationAsc()
    {
        var arrangementId = Guid.NewGuid();
        var beer1 = Guid.NewGuid();
        var beer2 = Guid.NewGuid();

        // Same TotalRating and RatingCount, beer2 has lower std dev (more consistent)
        await SeedResultAsync(arrangementId, beer1, 8.0m, ratingCount: 2, stdDev: 1.5m, beerName: "High StdDev");
        await SeedResultAsync(arrangementId, beer2, 8.0m, ratingCount: 2, stdDev: 0.5m, beerName: "Low StdDev");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangementId });

        Assert.Equal(1, response.Results[0].Rank);
        Assert.Equal("Low StdDev", response.Results[0].BeerNameSnapshot);
    }

    [Fact]
    public async Task HandleAsync_TieBreak_BeerIdAsc_IsDeterministic()
    {
        var arrangementId = Guid.NewGuid();
        // Create two beers with all equal stats — tie-break by BeerId ASC
        var beer1 = new Guid("00000000-0000-0000-0000-000000000001");
        var beer2 = new Guid("00000000-0000-0000-0000-000000000002");

        await SeedResultAsync(arrangementId, beer2, 8.0m, ratingCount: 2, stdDev: 0.5m, beerName: "Beer2");
        await SeedResultAsync(arrangementId, beer1, 8.0m, ratingCount: 2, stdDev: 0.5m, beerName: "Beer1");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangementId });

        Assert.Equal(1, response.Results[0].Rank);
        Assert.Equal(beer1, response.Results[0].BeerId);
    }

    [Fact]
    public async Task HandleAsync_OnlyReturnsResultsForRequestedArrangement()
    {
        var arrangement1 = Guid.NewGuid();
        var arrangement2 = Guid.NewGuid();

        await SeedResultAsync(arrangement1, Guid.NewGuid(), 8.0m, beerName: "Arr1 Beer");
        await SeedResultAsync(arrangement2, Guid.NewGuid(), 9.0m, beerName: "Arr2 Beer");

        var response = await _handler.HandleAsync(new GetResultsQuery { ArrangementId = arrangement1 });

        Assert.Single(response.Results);
        Assert.Equal("Arr1 Beer", response.Results[0].BeerNameSnapshot);
    }
}
