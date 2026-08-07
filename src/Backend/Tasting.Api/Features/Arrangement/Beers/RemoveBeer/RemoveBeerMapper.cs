using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Beers.RemoveBeer;

public sealed class RemoveBeerMapper
    : BaseCommandMapper<RemoveBeerRequest, ArrangementResponse, RemoveBeerCommand, Domain.Arrangement>
{
    public override RemoveBeerCommand ToCommand(RemoveBeerRequest req)
        => new(Guid.Empty, Guid.Empty, req.RowVersion);

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
