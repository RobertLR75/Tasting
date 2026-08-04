using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;

public sealed class UpdateArrangementMapper
    : BaseCommandMapper<UpdateArrangementRequest, ArrangementResponse, UpdateArrangementCommand, Domain.Arrangement>
{
    public override UpdateArrangementCommand ToCommand(UpdateArrangementRequest req)
        => new(Guid.Empty, req.Name, req.Description, req.RowVersion);

    public override Task<ArrangementResponse> FromEntityAsync(Domain.Arrangement entity, CancellationToken ct = default)
        => Task.FromResult(new ArrangementResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.RowVersion,
            entity.CreatedAt,
            entity.UpdatedAt));
}
