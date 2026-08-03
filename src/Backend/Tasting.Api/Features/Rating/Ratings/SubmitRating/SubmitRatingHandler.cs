using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Contracts;
using Tasting.Api.Infrastructure.Rating;
using Tasting.Api.Infrastructure.Rating.Entities;
using RatingEntity = Tasting.Api.Infrastructure.Rating.Entities.Rating;

namespace Tasting.Api.Features.Rating.Ratings.SubmitRating;

public class SubmitRatingHandler(RatingDbContext db, IArrangementService arrangementService)
    : IRequestHandler<SubmitRatingCommand, RatingEntity>
{
    public async Task<RatingEntity> HandleAsync(SubmitRatingCommand command, CancellationToken ct = default)
    {
        // 1. Verify arrangement is in Started status
        var status = await arrangementService.GetStatusAsync(command.ArrangementId, ct);
        if (status != ArrangementStatus.Started)
            throw new ConflictException("Arrangement is not in Started status.");

        // 2. Verify caller is a participant
        if (!await arrangementService.IsParticipantAsync(command.ArrangementId, command.ParticipantId, ct))
            throw new ForbiddenException("User is not a participant in this arrangement.");

        // 3. Verify beer is in the arrangement
        if (!await arrangementService.IsBeerInArrangementAsync(command.ArrangementId, command.BeerId, ct))
            throw new ServiceNotFoundException("Beer is not in this arrangement.");

        // 4. Validate sub-scores: [0, 10] with 0.5 step
        ValidateScore(command.Visibility, "Visibility");
        ValidateScore(command.Smell, "Smell");
        ValidateScore(command.Taste, "Taste");
        ValidateScore(command.Toast, "Toast");

        // 5 & 6. Calculate TotalRating server-side, rounded to 2dp (ADR-0017, ADR-0023)
        var total = Math.Round(
            (command.Visibility + command.Smell + command.Taste + command.Toast) / 4m,
            2,
            MidpointRounding.AwayFromZero);

        // 7 & 8. Upsert with optimistic concurrency (ADR-0010, ADR-0030)
        var existing = await db.Ratings
            .FirstOrDefaultAsync(
                r => r.ArrangementId == command.ArrangementId
                     && r.ParticipantId == command.ParticipantId
                     && r.BeerId == command.BeerId,
                ct);

        RatingEntity rating;
        if (existing is null)
        {
            rating = new RatingEntity
            {
                Id = Guid.CreateVersion7(),
                ArrangementId = command.ArrangementId,
                ParticipantId = command.ParticipantId,
                BeerId = command.BeerId,
                Visibility = command.Visibility,
                Smell = command.Smell,
                Taste = command.Taste,
                Toast = command.Toast,
                TotalRating = total,
                RowVersion = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Ratings.Add(rating);
        }
        else
        {
            existing.Visibility = command.Visibility;
            existing.Smell = command.Smell;
            existing.Taste = command.Taste;
            existing.Toast = command.Toast;
            existing.TotalRating = total;
            existing.RowVersion++;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            rating = existing;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("A concurrent update was detected on the rating. Please retry.");
        }

        // 9 & 10. Auto-create/update Result and ResultParticipant (ADR-0011)
        await UpdateResultAsync(command, rating, ct);

        return rating;
    }

    private async Task UpdateResultAsync(SubmitRatingCommand command, RatingEntity rating, CancellationToken ct)
    {
        // Load all current ratings for this arrangement+beer to recalculate aggregates
        var allRatings = await db.Ratings
            .Where(r => r.ArrangementId == command.ArrangementId && r.BeerId == command.BeerId)
            .ToListAsync(ct);

        var count = allRatings.Count;
        var mean = Math.Round(allRatings.Sum(r => r.TotalRating) / count, 2, MidpointRounding.AwayFromZero);

        // Population standard deviation — stored without rounding (ADR-0023)
        var variance = count > 1
            ? allRatings.Sum(r => (r.TotalRating - mean) * (r.TotalRating - mean)) / count
            : 0m;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        var existingResult = await db.Results
            .FirstOrDefaultAsync(
                r => r.ArrangementId == command.ArrangementId && r.BeerId == command.BeerId,
                ct);

        if (existingResult is null)
        {
            var beerName = await arrangementService.GetBeerNameSnapshotAsync(command.ArrangementId, command.BeerId, ct);
            existingResult = new Result
            {
                Id = Guid.CreateVersion7(),
                ArrangementId = command.ArrangementId,
                BeerId = command.BeerId,
                BeerNameSnapshot = beerName ?? string.Empty,
                TotalRating = mean,
                RatingCount = count,
                StandardDeviation = stdDev,
                Rank = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Results.Add(existingResult);
        }
        else
        {
            existingResult.TotalRating = mean;
            existingResult.RatingCount = count;
            existingResult.StandardDeviation = stdDev;
            existingResult.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        // Upsert ResultParticipant
        var existingParticipant = await db.ResultParticipants
            .FirstOrDefaultAsync(
                rp => rp.ResultId == existingResult.Id && rp.ParticipantId == command.ParticipantId,
                ct);

        if (existingParticipant is null)
        {
            var participantName = await arrangementService.GetParticipantNameSnapshotAsync(
                command.ArrangementId, command.ParticipantId, ct);

            db.ResultParticipants.Add(new ResultParticipant
            {
                Id = Guid.CreateVersion7(),
                ResultId = existingResult.Id,
                ParticipantId = command.ParticipantId,
                ParticipantNameSnapshot = participantName ?? string.Empty,
                Rating = rating.TotalRating
            });
        }
        else
        {
            existingParticipant.Rating = rating.TotalRating;
        }

        await db.SaveChangesAsync(ct);

        // Recalculate ranks for all results in this arrangement (ADR-0012)
        await RecalculateRanksAsync(command.ArrangementId, ct);
    }

    private async Task RecalculateRanksAsync(Guid arrangementId, CancellationToken ct)
    {
        var results = await db.Results
            .Where(r => r.ArrangementId == arrangementId)
            .ToListAsync(ct);

        var ranked = results
            .OrderByDescending(r => r.TotalRating)
            .ThenByDescending(r => r.RatingCount)
            .ThenBy(r => r.StandardDeviation)
            .ThenBy(r => r.BeerId)
            .ToList();

        for (var i = 0; i < ranked.Count; i++)
        {
            ranked[i].Rank = i + 1;
        }

        await db.SaveChangesAsync(ct);
    }

    private static void ValidateScore(decimal score, string fieldName)
    {
        if (score < 0 || score > 10)
            throw new ValidationException($"{fieldName} must be between 0 and 10.");

        if (score % 0.5m != 0)
            throw new ValidationException($"{fieldName} must be in increments of 0.5.");
    }
}
