using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerMapper : BaseCommandMapper<CreateBeerRequest, CreateBeerResponse, CreateBeerCommand, Beer>
{
    public override CreateBeerCommand ToCommand(CreateBeerRequest req)
    {
        return new CreateBeerCommand(req.BreweryId, req.BeerStyleId, req.BeerTypeId, req.Name, req.IsActive);
    }

    public override CreateBeerResponse FromEntity(Beer entity)
    {
        return new CreateBeerResponse
        {
            Id = entity.Id,
            BreweryId = entity.BreweryId,
            BeerStyleId = entity.BeerStyleId,
            BeerTypeId = entity.BeerTypeId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public override Task<CreateBeerResponse> FromEntityAsync(Beer entity, CancellationToken ct = default)
    {
        return Task.FromResult(FromEntity(entity));
    }
}
