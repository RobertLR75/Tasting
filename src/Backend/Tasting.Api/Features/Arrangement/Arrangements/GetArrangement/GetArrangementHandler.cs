using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;

public sealed class GetArrangementHandler(ArrangementDbContext dbContext)
    : IRequestHandler<GetArrangementQuery, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        GetArrangementQuery request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .AsNoTracking()
            .Include(a => a.Participants)
            .Include(a => a.Beers)
            .FirstOrDefaultAsync(a => a.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        return arrangement.ToDomain();
    }
}
