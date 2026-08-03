using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Rating;

namespace Tasting.Api.Features.Rating.Results.GetResults;

public class GetResultsHandler(RatingDbContext db) : IRequestHandler<GetResultsQuery, GetResultsResponse>
{
    public async Task<GetResultsResponse> HandleAsync(GetResultsQuery query, CancellationToken ct = default)
    {
        var results = await db.Results
            .Where(r => r.ArrangementId == query.ArrangementId)
            .Include(r => r.Participants)
            .AsNoTracking()
            .ToListAsync(ct);

        // Ranking: TotalRating DESC, RatingCount DESC, StandardDeviation ASC, BeerId ASC (ADR-0012)
        var ranked = results
            .OrderByDescending(r => r.TotalRating)
            .ThenByDescending(r => r.RatingCount)
            .ThenBy(r => r.StandardDeviation)
            .ThenBy(r => r.BeerId)
            .Select((r, i) => new GetResultItem
            {
                Rank = i + 1,
                BeerId = r.BeerId,
                BeerNameSnapshot = r.BeerNameSnapshot,
                TotalRating = r.TotalRating,
                RatingCount = r.RatingCount,
                StandardDeviation = r.StandardDeviation,
                Participants = r.Participants
                    .Select(p => new GetResultParticipantItem
                    {
                        ParticipantId = p.ParticipantId,
                        ParticipantNameSnapshot = p.ParticipantNameSnapshot,
                        Rating = p.Rating
                    })
                    .ToList()
            })
            .ToList();

        return new GetResultsResponse { Results = ranked };
    }
}
