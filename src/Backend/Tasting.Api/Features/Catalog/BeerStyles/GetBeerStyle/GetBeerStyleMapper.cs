using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.BeerStyles;

namespace Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;

public sealed class GetBeerStyleMapper : BaseQueryMapper<GetBeerStyleRequest, BeerStyleResponse, GetBeerStyleQuery, BeerStyleResponse>
{
    public override GetBeerStyleQuery ToQuery(GetBeerStyleRequest req) => new(req.Id);

    public override BeerStyleResponse FromEntity(BeerStyleResponse entity) => entity;

    public override Task<BeerStyleResponse> FromEntityAsync(BeerStyleResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
