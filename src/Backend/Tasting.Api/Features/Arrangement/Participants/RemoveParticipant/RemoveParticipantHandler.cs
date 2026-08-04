using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;

public sealed class RemoveParticipantHandler(ArrangementDbContext dbContext)
    : IRequestHandler<RemoveParticipantCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        RemoveParticipantCommand request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        if (arrangement.Status != ArrangementStatus.Created)
        {
            throw new ConflictException(
                $"Participants can only be removed when arrangement is in 'Created' status. Current status: '{arrangement.Status}'.");
        }

        if (arrangement.RowVersion != request.RowVersion)
        {
            throw new ConflictException(
                "Arrangement has been modified by another request. Please reload and retry.");
        }

        var participant = arrangement.Participants
            .FirstOrDefault(p => p.UserId == request.UserId)
            ?? throw new ServiceNotFoundException($"Participant '{request.UserId}' was not found in arrangement '{request.ArrangementId}'.");

        arrangement.Participants.Remove(participant);
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
}
