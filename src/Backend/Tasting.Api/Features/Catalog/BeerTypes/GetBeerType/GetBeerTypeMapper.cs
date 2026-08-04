using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.BeerTypes;

namespace Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;

public sealed class GetBeerTypeMapper : BaseQueryMapper<GetBeerTypeRequest, BeerTypeResponse, GetBeerTypeQuery, BeerTypeResponse>
{
    public override GetBeerTypeQuery ToQuery(GetBeerTypeRequest req) => new(req.Id);

    public override BeerTypeResponse FromEntity(BeerTypeResponse entity) => entity;

    public override Task<BeerTypeResponse> FromEntityAsync(BeerTypeResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
