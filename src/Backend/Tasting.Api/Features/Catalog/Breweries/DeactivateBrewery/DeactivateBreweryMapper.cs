using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;

public sealed class DeactivateBreweryMapper : BaseCommandMapper<DeactivateBreweryRequest, BreweryResponse, DeactivateBreweryCommand, Brewery>
{
    public override DeactivateBreweryCommand ToCommand(DeactivateBreweryRequest req) => new(req.Id);

    public override BreweryResponse FromEntity(Brewery entity)
        => new(entity.Id, entity.Name, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BreweryResponse> FromEntityAsync(Brewery entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
