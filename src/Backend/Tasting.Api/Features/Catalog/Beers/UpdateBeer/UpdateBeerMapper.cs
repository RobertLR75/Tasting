using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.UpdateBeer;

public sealed class UpdateBeerMapper : BaseCommandMapper<UpdateBeerRequest, BeerResponse, UpdateBeerCommand, Beer>
{
    public override UpdateBeerCommand ToCommand(UpdateBeerRequest req)
        => new(req.Id, req.BreweryId, req.BeerStyleId, req.BeerTypeId, req.Name, req.IsActive);

    public override BeerResponse FromEntity(Beer entity)
        => new(entity.Id, entity.BreweryId, entity.BeerStyleId, entity.BeerTypeId, entity.Name, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BeerResponse> FromEntityAsync(Beer entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
