using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Breweries;

namespace Tasting.Api.Features.Catalog.Breweries.GetBrewery;

public sealed class GetBreweryMapper : BaseQueryMapper<GetBreweryRequest, BreweryResponse, GetBreweryQuery, BreweryResponse>
{
    public override GetBreweryQuery ToQuery(GetBreweryRequest req) => new(req.Id);

    public override BreweryResponse FromEntity(BreweryResponse entity) => entity;

    public override Task<BreweryResponse> FromEntityAsync(BreweryResponse entity, CancellationToken ct = default)
        => Task.FromResult(entity);
}
