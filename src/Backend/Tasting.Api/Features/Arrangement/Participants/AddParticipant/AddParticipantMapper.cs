using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Participants.AddParticipant;

public sealed class AddParticipantMapper
    : BaseCommandMapper<AddParticipantRequest, ArrangementResponse, AddParticipantCommand, Domain.Arrangement>
{
    public override AddParticipantCommand ToCommand(AddParticipantRequest req)
        => new(Guid.Empty, req.UserId, req.RowVersion);

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
            entity.Participants
                .Select(p => new ArrangementParticipantResponse(p.Id, p.UserId, $"{p.FirstNameSnapshot} {p.LastNameSnapshot}".Trim()))
                .ToList()));
}
