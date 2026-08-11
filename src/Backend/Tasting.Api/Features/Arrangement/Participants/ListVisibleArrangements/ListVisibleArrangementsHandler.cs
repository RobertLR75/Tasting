using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Participants.ListVisibleArrangements;

public sealed class ListVisibleArrangementsHandler(ArrangementDbContext dbContext)
    : IRequestHandler<ListVisibleArrangementsQuery, ListVisibleArrangementsResponse>
{
    public async Task<ListVisibleArrangementsResponse> HandleAsync(
        ListVisibleArrangementsQuery request,
        CancellationToken ct = default)
    {
        var items = await dbContext.Arrangements
            .AsNoTracking()
            .Where(arrangement => arrangement.Status == ArrangementStatus.Active)
            .OrderByDescending(arrangement => arrangement.CreatedAt)
            .Select(arrangement => new VisibleArrangementResponse(
                arrangement.Id,
                arrangement.Name,
                arrangement.Description,
                arrangement.Participants.Any(participant => participant.UserId == request.UserId)))
            .ToListAsync(ct);

        return new ListVisibleArrangementsResponse(items);
    }
}
