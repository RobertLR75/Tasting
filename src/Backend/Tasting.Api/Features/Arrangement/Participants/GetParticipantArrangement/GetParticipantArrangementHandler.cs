using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Participants.GetParticipantArrangement;

public sealed class GetParticipantArrangementHandler(ArrangementDbContext dbContext)
    : IRequestHandler<GetParticipantArrangementQuery, ParticipantArrangementResponse>
{
    public async Task<ParticipantArrangementResponse> HandleAsync(
        GetParticipantArrangementQuery request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        var isParticipant = await dbContext.Participants
            .AsNoTracking()
            .AnyAsync(
                participant => participant.ArrangementId == request.ArrangementId && participant.UserId == request.UserId,
                ct);

        if (!isParticipant)
        {
            throw new ForbiddenException("You are not a participant in this arrangement.");
        }

        if (arrangement.Status == ArrangementStatus.Canceled)
        {
            throw new ConflictException("The arrangement is canceled.");
        }

        ParticipantBeerResponse[] beers = [];
        if (arrangement.Status is ArrangementStatus.Started or ArrangementStatus.Completed)
        {
            beers = await dbContext.Beers
                .AsNoTracking()
                .Where(beer => beer.ArrangementId == arrangement.Id)
                .Select(beer => new ParticipantBeerResponse(
                    beer.BeerId,
                    beer.NameSnapshot,
                    beer.BreweryNameSnapshot,
                    beer.BeerStyleSnapshot,
                    beer.BeerTypeSnapshot))
                .ToArrayAsync(ct);
        }

        return new ParticipantArrangementResponse(arrangement.Id, arrangement.Name, arrangement.Status, beers);
    }
}
