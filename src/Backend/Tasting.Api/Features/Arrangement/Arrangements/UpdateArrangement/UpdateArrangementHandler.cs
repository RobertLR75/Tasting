using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;

public sealed class UpdateArrangementHandler(ArrangementDbContext dbContext)
    : IRequestHandler<UpdateArrangementCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        UpdateArrangementCommand request,
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
                $"Arrangement cannot be updated in status '{arrangement.Status}'. Only 'Created' arrangements can be updated.");
        }
        arrangement.Name = request.Name.Trim();
        arrangement.Description = request.Description?.Trim();
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
