using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Beers.AddBeer;

public sealed class AddBeerMapper
    : BaseCommandMapper<AddBeerRequest, ArrangementResponse, AddBeerCommand, Domain.Arrangement>
{
    public override AddBeerCommand ToCommand(AddBeerRequest req)
        => new(Guid.Empty, req.BeerId, req.RowVersion);

    public override Task<ArrangementResponse> FromEntityAsync(
        Domain.Arrangement entity,
        CancellationToken ct = default)
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
