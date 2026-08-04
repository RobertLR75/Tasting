using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Beers;

namespace Tasting.Api.Features.Catalog.Beers.GetBeer;

public sealed class GetBeerMapper : BaseQueryMapper<GetBeerRequest, BeerResponse, GetBeerQuery, BeerResponse>
{
    public override GetBeerQuery ToQuery(GetBeerRequest req) => new(req.Id);

    public override BeerResponse FromEntity(BeerResponse entity) => entity;

    public override Task<BeerResponse> FromEntityAsync(BeerResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
