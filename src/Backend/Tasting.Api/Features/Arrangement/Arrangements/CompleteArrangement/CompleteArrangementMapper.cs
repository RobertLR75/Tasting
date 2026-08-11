using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.CompleteArrangement;

public sealed class CompleteArrangementMapper
    : BaseCommandMapper<CompleteArrangementRequest, ArrangementResponse, CompleteArrangementCommand, Domain.Arrangement>
{
    public override CompleteArrangementCommand ToCommand(CompleteArrangementRequest req)
        => new(Guid.Empty);

    public override Task<ArrangementResponse> FromEntityAsync(Domain.Arrangement entity, CancellationToken ct = default)
        => Task.FromResult(new ArrangementResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Beers.Select(b => new ArrangementBeerItem(b.Id, b.BeerId, b.NameSnapshot)).ToList(),
            entity.Participants
                .Select(p => new ArrangementParticipantResponse(p.Id, p.UserId, $"{p.FirstNameSnapshot} {p.LastNameSnapshot}".Trim()))
                .ToList()));
}
