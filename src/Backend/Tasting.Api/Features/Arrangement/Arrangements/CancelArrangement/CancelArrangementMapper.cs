using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;

public sealed class CancelArrangementMapper
    : BaseCommandMapper<CancelArrangementRequest, ArrangementResponse, CancelArrangementCommand, Domain.Arrangement>
{
    public override CancelArrangementCommand ToCommand(CancelArrangementRequest req)
        => new(Guid.Empty, req.RowVersion);

    public override Task<ArrangementResponse> FromEntityAsync(Domain.Arrangement entity, CancellationToken ct = default)
        => Task.FromResult(new ArrangementResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.RowVersion,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Beers.Select(b => new ArrangementBeerItem(b.Id, b.BeerId, b.NameSnapshot)).ToList()));
}
