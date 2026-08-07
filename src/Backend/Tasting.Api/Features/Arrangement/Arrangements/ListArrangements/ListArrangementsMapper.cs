using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed class ListArrangementsMapper
    : BaseQueryMapper<ListArrangementsRequest, ListArrangementsResponse, ListArrangementsQuery, ListArrangementsResult>
{
    public override ListArrangementsQuery ToQuery(ListArrangementsRequest req)
        => new(req.Status);

    public override Task<ListArrangementsResponse> FromEntityAsync(ListArrangementsResult entity, CancellationToken ct = default)
        => Task.FromResult(new ListArrangementsResponse(
            entity.Items.Select(a => new ArrangementResponse(
                a.Id,
                a.Name,
                a.Description,
                a.Status,
                a.RowVersion,
                a.CreatedAt,
                a.UpdatedAt,
                a.Participants
                    .Select(p => new ArrangementParticipantResponse(p.Id, p.UserId, $"{p.FirstNameSnapshot} {p.LastNameSnapshot}".Trim()))
                    .ToList())).ToList()));
}
