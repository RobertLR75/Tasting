using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Rating.Ratings.SubmitRating;
using Tasting.Api.Infrastructure.Rating;
using Tasting.Api.Infrastructure.Rating.Entities;

namespace Tasting.Api.UnitTests.Rating;

public class SubmitRatingHandlerTests : IDisposable
{
    private readonly RatingDbContext _db;
    private readonly IArrangementService _arrangementService;
    private readonly SubmitRatingHandler _handler;

    public SubmitRatingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<RatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new RatingDbContext(options);

        _arrangementService = Substitute.For<IArrangementService>();
        _arrangementService.GetStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ArrangementStatus.Started);
        _arrangementService.IsParticipantAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _arrangementService.IsBeerInArrangementAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _arrangementService.GetBeerNameSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("Test Beer");
        _arrangementService.GetParticipantNameSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("Test User");

        _handler = new SubmitRatingHandler(_db, _arrangementService);
    }

    public void Dispose() => _db.Dispose();

    private static SubmitRatingCommand ValidCommand(Guid? arrangementId = null, Guid? participantId = null, Guid? beerId = null) =>
        new()
        {
            ArrangementId = arrangementId ?? Guid.NewGuid(),
            ParticipantId = participantId ?? Guid.NewGuid(),
            BeerId = beerId ?? Guid.NewGuid(),
            Visibility = 8.0m,
            Smell = 7.5m,
            Taste = 9.0m,
            Toast = 8.5m
        };

    [Fact]
    public async Task HandleAsync_ValidScores_CalculatesTotalRating()
    {
        var cmd = ValidCommand();
        var rating = await _handler.HandleAsync(cmd);

        // (8.0 + 7.5 + 9.0 + 8.5) / 4 = 33.0 / 4 = 8.25
        Assert.Equal(8.25m, rating.TotalRating);
    }

    [Fact]
    public async Task HandleAsync_ValidScores_CreatesResultRow()
    {
        var cmd = ValidCommand();
        await _handler.HandleAsync(cmd);

        var result = await _db.Results.FirstAsync(r => r.ArrangementId == cmd.ArrangementId && r.BeerId == cmd.BeerId);
        Assert.Equal(8.25m, result.TotalRating);
        Assert.Equal(1, result.RatingCount);
    }

    [Fact]
    public async Task HandleAsync_DuplicateSubmit_UpdatesRatingInsteadOfCreating()
    {
        var arrangementId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var beerId = Guid.NewGuid();

        var first = ValidCommand(arrangementId, participantId, beerId);
        await _handler.HandleAsync(first);

        var second = first with { Visibility = 5.0m, Smell = 5.0m, Taste = 5.0m, Toast = 5.0m };
        await _handler.HandleAsync(second);

        var ratings = await _db.Ratings
            .Where(r => r.ArrangementId == arrangementId && r.ParticipantId == participantId && r.BeerId == beerId)
            .ToListAsync();

        Assert.Single(ratings); // upsert — only one row
        Assert.Equal(5.0m, ratings[0].TotalRating);
        Assert.NotNull(ratings[0].UpdatedAt);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(10.5)]
    [InlineData(11.0)]
    public async Task HandleAsync_ScoreOutOfRange_ThrowsValidationException(double score)
    {
        var cmd = ValidCommand() with { Visibility = (decimal)score };
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(
            () => _handler.HandleAsync(cmd));
    }

    [Theory]
    [InlineData(0.3)]
    [InlineData(1.7)]
    [InlineData(5.25)]
    public async Task HandleAsync_ScoreNotHalfStep_ThrowsValidationException(double score)
    {
        var cmd = ValidCommand() with { Smell = (decimal)score };
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(
            () => _handler.HandleAsync(cmd));
    }

    [Fact]
    public async Task HandleAsync_ArrangementNotStarted_ThrowsConflictException()
    {
        _arrangementService.GetStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ArrangementStatus.Created);

        await Assert.ThrowsAsync<ConflictException>(() => _handler.HandleAsync(ValidCommand()));
    }

    [Fact]
    public async Task HandleAsync_CallerNotParticipant_ThrowsForbiddenException()
    {
        _arrangementService.IsParticipantAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.HandleAsync(ValidCommand()));
    }

    [Fact]
    public async Task HandleAsync_BeerNotInArrangement_ThrowsNotFoundException()
    {
        _arrangementService.IsBeerInArrangementAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => _handler.HandleAsync(ValidCommand()));
    }

    [Fact]
    public async Task HandleAsync_TotalRatingRounding_UsesAwayFromZero()
    {
        // (7.5 + 7.5 + 7.5 + 7.5) / 4 = 7.5 — no rounding needed
        // Use values where MidpointRounding matters: (6.5 + 6.5 + 6.5 + 6.5) / 4 = 6.5 → 6.50
        var cmd = ValidCommand() with { Visibility = 6.5m, Smell = 6.5m, Taste = 6.5m, Toast = 6.5m };
        var rating = await _handler.HandleAsync(cmd);
        Assert.Equal(6.50m, rating.TotalRating);
    }

    [Fact]
    public async Task HandleAsync_MultipleRatingsForSameBeer_UpdatesResultAggregate()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();

        // Participant 1: total = 8.0
        await _handler.HandleAsync(ValidCommand(arrangementId, beerId: beerId) with
        {
            Visibility = 8.0m, Smell = 8.0m, Taste = 8.0m, Toast = 8.0m
        });

        // Participant 2: total = 6.0
        await _handler.HandleAsync(ValidCommand(arrangementId, beerId: beerId) with
        {
            Visibility = 6.0m, Smell = 6.0m, Taste = 6.0m, Toast = 6.0m
        });

        var result = await _db.Results.FirstAsync(r => r.ArrangementId == arrangementId && r.BeerId == beerId);
        Assert.Equal(2, result.RatingCount);
        Assert.Equal(7.0m, result.TotalRating); // (8.0 + 6.0) / 2
    }

    [Fact]
    public async Task HandleAsync_NewRatingIncrements_RowVersionToOne()
    {
        var cmd = ValidCommand();
        var rating = await _handler.HandleAsync(cmd);
        Assert.Equal(1u, rating.RowVersion);
    }

    [Fact]
    public async Task HandleAsync_UpdatedRating_IncrementsRowVersion()
    {
        var arrangementId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        var cmd = ValidCommand(arrangementId, participantId, beerId);

        await _handler.HandleAsync(cmd);
        var updated = await _handler.HandleAsync(cmd);

        Assert.Equal(2u, updated.RowVersion);
    }
}
