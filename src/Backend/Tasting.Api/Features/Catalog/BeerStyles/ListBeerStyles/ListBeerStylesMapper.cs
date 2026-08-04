using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesMapper : BaseQueryMapper<ListBeerStylesRequest, ListBeerStylesResponse, ListBeerStylesQuery, ListBeerStylesResponse>
{
    public override ListBeerStylesQuery ToQuery(ListBeerStylesRequest req) => new();

    public override ListBeerStylesResponse FromEntity(ListBeerStylesResponse entity) => entity;

    public override Task<ListBeerStylesResponse> FromEntityAsync(ListBeerStylesResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
