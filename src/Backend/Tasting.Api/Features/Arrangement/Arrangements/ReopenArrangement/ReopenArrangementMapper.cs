using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.ReopenArrangement;

public sealed class ReopenArrangementMapper
    : BaseCommandMapper<ReopenArrangementRequest, ArrangementResponse, ReopenArrangementCommand, Domain.Arrangement>
{
    public override ReopenArrangementCommand ToCommand(ReopenArrangementRequest req)
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
            entity.Beers.Select(b => new ArrangementBeerItem(b.Id, b.BeerId, b.NameSnapshot)).ToList(),
            entity.Participants
                .Select(p => new ArrangementParticipantResponse(p.Id, p.UserId, $"{p.FirstNameSnapshot} {p.LastNameSnapshot}".Trim()))
                .ToList()));
}
