using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.DeactivateBeer;

public sealed class DeactivateBeerMapper : BaseCommandMapper<DeactivateBeerRequest, BeerResponse, DeactivateBeerCommand, Beer>
{
    public override DeactivateBeerCommand ToCommand(DeactivateBeerRequest req) => new(req.Id);

    public override BeerResponse FromEntity(Beer entity)
        => new(entity.Id, entity.BreweryId, entity.BeerStyleId, entity.BeerTypeId, entity.Name, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BeerResponse> FromEntityAsync(Beer entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
