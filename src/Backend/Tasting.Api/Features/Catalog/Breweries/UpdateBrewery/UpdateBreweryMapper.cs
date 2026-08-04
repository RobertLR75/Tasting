using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed class UpdateBreweryMapper : BaseCommandMapper<UpdateBreweryRequest, BreweryResponse, UpdateBreweryCommand, Brewery>
{
    public override UpdateBreweryCommand ToCommand(UpdateBreweryRequest req) => new(req.Id, req.Name, req.IsActive);

    public override BreweryResponse FromEntity(Brewery entity)
        => new(entity.Id, entity.Name, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BreweryResponse> FromEntityAsync(Brewery entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
