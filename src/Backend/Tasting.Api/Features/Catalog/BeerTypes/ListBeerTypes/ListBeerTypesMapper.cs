using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesMapper : BaseQueryMapper<ListBeerTypesRequest, ListBeerTypesResponse, ListBeerTypesQuery, ListBeerTypesResponse>
{
    public override ListBeerTypesQuery ToQuery(ListBeerTypesRequest req) => new();

    public override ListBeerTypesResponse FromEntity(ListBeerTypesResponse entity) => entity;

    public override Task<ListBeerTypesResponse> FromEntityAsync(ListBeerTypesResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
