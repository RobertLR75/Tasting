using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed class ListBreweriesMapper : BaseQueryMapper<ListBreweriesRequest, ListBreweriesResponse, ListBreweriesQuery, ListBreweriesResponse>
{
    public override ListBreweriesQuery ToQuery(ListBreweriesRequest req) => new(req.IncludeInactive);

    public override ListBreweriesResponse FromEntity(ListBreweriesResponse entity) => entity;

    public override Task<ListBreweriesResponse> FromEntityAsync(ListBreweriesResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
