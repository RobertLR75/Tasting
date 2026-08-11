using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;

public sealed class CreateArrangementMapper
    : BaseCommandMapper<CreateArrangementRequest, ArrangementResponse, CreateArrangementCommand, Domain.Arrangement>
{
    public override CreateArrangementCommand ToCommand(CreateArrangementRequest req)
        => new(req.Name, req.Description);

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
