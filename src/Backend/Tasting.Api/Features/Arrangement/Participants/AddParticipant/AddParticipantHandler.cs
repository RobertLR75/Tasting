using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Arrangement.Participants.AddParticipant;

public sealed class AddParticipantHandler(
    ArrangementDbContext dbContext,
    UsersDbContext usersDbContext)
    : IRequestHandler<AddParticipantCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        AddParticipantCommand request,
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
                "Participants can only be added when arrangement is in Created status.");
        }
        var userExists = await usersDbContext.Users
            .AnyAsync(u => u.Id == request.UserId, ct);
        if (!userExists)
        {
            throw new ServiceNotFoundException($"User '{request.UserId}' was not found.");
        }

        var alreadyAdded = arrangement.Participants
            .Any(p => p.UserId == request.UserId);
        if (alreadyAdded)
        {
            throw new ConflictException(
                "This participant is already added to the arrangement.");
        }

        arrangement.Participants.Add(new ArrangementParticipant
        {
            Id = Guid.CreateVersion7(),
            ArrangementId = request.ArrangementId,
            UserId = request.UserId,
            FirstNameSnapshot = string.Empty,
            LastNameSnapshot = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        });

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

        return arrangement.ToDomain();
    }
}
