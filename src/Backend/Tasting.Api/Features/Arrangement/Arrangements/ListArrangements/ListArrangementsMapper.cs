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
                a.Beers.Select(b => new ArrangementBeerItem(b.Id, b.BeerId, b.NameSnapshot)).ToList())).ToList()));
}
