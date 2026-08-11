using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed class ListArrangementsHandler(ArrangementDbContext dbContext)
    : IRequestHandler<ListArrangementsQuery, ListArrangementsResult>
{
    public async Task<ListArrangementsResult> HandleAsync(
        ListArrangementsQuery request,
        CancellationToken ct = default)
    {
        var query = dbContext.Arrangements
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return new ListArrangementsResult(items.Select(item => item.ToDomain()).ToList());
    }
}
