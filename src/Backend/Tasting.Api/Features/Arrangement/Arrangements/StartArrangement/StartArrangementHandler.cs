using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;

public sealed class StartArrangementHandler(
    ArrangementDbContext dbContext,
    UsersDbContext usersDbContext,
    CatalogDbContext catalogDbContext)
    : IRequestHandler<StartArrangementCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        StartArrangementCommand request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .Include(a => a.Participants)
            .Include(a => a.Beers)
            .FirstOrDefaultAsync(a => a.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        if (arrangement.Status != ArrangementStatus.Created)
        {
            throw new ConflictException(
                $"Arrangement cannot be started from status '{arrangement.Status}'. Only 'Created' arrangements can be started.");
        }

        if (arrangement.RowVersion != request.RowVersion)
        {
            throw new ConflictException(
                "Arrangement has been modified by another request. Please reload and retry.");
        }

        await TakeParticipantSnapshotsAsync(arrangement, ct);
        await TakeBeerSnapshotsAsync(arrangement, ct);

        arrangement.Status = ArrangementStatus.Started;
        arrangement.RowVersion++;
        arrangement.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Arrangement was modified concurrently. Please reload and retry.");
        }

        return arrangement;
    }

    private async Task TakeParticipantSnapshotsAsync(Domain.Arrangement arrangement, CancellationToken ct)
    {
        var userIds = arrangement.Participants.Select(p => p.UserId).ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var users = await usersDbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(ct);

        var userMap = users.ToDictionary(u => u.Id);

        foreach (var participant in arrangement.Participants)
        {
            if (userMap.TryGetValue(participant.UserId, out var user))
            {
                participant.FirstNameSnapshot = user.FirstName;
                participant.LastNameSnapshot = user.LastName;
            }
        }
    }

    private async Task TakeBeerSnapshotsAsync(Domain.Arrangement arrangement, CancellationToken ct)
    {
        var beerIds = arrangement.Beers.Select(b => b.BeerId).ToList();
        if (beerIds.Count == 0)
        {
            return;
        }

        var beers = await catalogDbContext.Beers
            .Include(b => b.Brewery)
            .Include(b => b.BeerStyle)
            .Include(b => b.BeerType)
            .Where(b => beerIds.Contains(b.Id))
            .ToListAsync(ct);

        var beerMap = beers.ToDictionary(b => b.Id);

        foreach (var arrangementBeer in arrangement.Beers)
        {
            if (beerMap.TryGetValue(arrangementBeer.BeerId, out var beer))
            {
                arrangementBeer.NameSnapshot = beer.Name;
                arrangementBeer.BreweryNameSnapshot = beer.Brewery?.Name ?? string.Empty;
                arrangementBeer.BeerStyleSnapshot = beer.BeerStyle?.Name ?? string.Empty;
                arrangementBeer.BeerTypeSnapshot = beer.BeerType?.Name ?? string.Empty;
            }
        }
    }
}
