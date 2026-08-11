using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Infrastructure.Arrangement;

namespace Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;

public sealed class CreateArrangementHandler(ArrangementDbContext dbContext)
    : IRequestHandler<CreateArrangementCommand, Domain.Arrangement>
{
    public async Task<Domain.Arrangement> HandleAsync(
        CreateArrangementCommand request,
        CancellationToken ct = default)
    {
        var arrangement = new Domain.Arrangement
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = ArrangementStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Arrangements.Add(ArrangementRecord.FromDomain(arrangement));
        await dbContext.SaveChangesAsync(ct);
        return arrangement;
    }
}
