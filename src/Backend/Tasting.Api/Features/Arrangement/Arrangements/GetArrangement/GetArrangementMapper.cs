using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;

public sealed class GetArrangementMapper
    : BaseQueryMapper<GetArrangementRequest, ArrangementResponse, GetArrangementQuery, Domain.Arrangement>
{
    public override GetArrangementQuery ToQuery(GetArrangementRequest req)
        => new(req.ArrangementId);

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
