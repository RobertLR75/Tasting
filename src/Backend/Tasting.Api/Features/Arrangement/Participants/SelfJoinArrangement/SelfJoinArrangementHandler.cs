using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Arrangement.Participants.SelfJoinArrangement;

public sealed class SelfJoinArrangementHandler(
    ArrangementDbContext dbContext,
    UsersDbContext usersDbContext)
    : IRequestHandler<SelfJoinArrangementCommand, SelfJoinArrangementResponse>
{
    public async Task<SelfJoinArrangementResponse> HandleAsync(
        SelfJoinArrangementCommand request,
        CancellationToken ct = default)
    {
        var arrangement = await dbContext.Arrangements
            .Include(item => item.Participants)
            .FirstOrDefaultAsync(item => item.Id == request.ArrangementId, ct)
            ?? throw new ServiceNotFoundException($"Arrangement '{request.ArrangementId}' was not found.");

        if (arrangement.Status != ArrangementStatus.Active)
        {
            throw new ConflictException("Participants can only self-join an active arrangement.");
        }

        if (arrangement.Participants.Any(participant => participant.UserId == request.UserId))
        {
            throw new ConflictException("You have already joined this arrangement.");
        }

        var user = await usersDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.UserId && item.IsActive, ct)
            ?? throw new ServiceNotFoundException($"User '{request.UserId}' was not found.");

        arrangement.Participants.Add(new ArrangementParticipant
        {
            Id = Guid.CreateVersion7(),
            ArrangementId = arrangement.Id,
            UserId = user.Id,
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
        catch (DbUpdateException)
        {
            throw new ConflictException("Unable to join the arrangement because membership changed concurrently.");
        }

        return new SelfJoinArrangementResponse(arrangement.Id, arrangement.Name, arrangement.Status);
    }
}
