using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryMapper : BaseCommandMapper<CreateBreweryRequest, CreateBreweryResponse, CreateBreweryCommand, Brewery>
{
    public override CreateBreweryCommand ToCommand(CreateBreweryRequest req)
    {
        return new CreateBreweryCommand(req.Name, req.IsActive);
    }

    public override CreateBreweryResponse FromEntity(Brewery entity)
    {
        return new CreateBreweryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public override Task<CreateBreweryResponse> FromEntityAsync(Brewery entity, CancellationToken ct = default)
    {
        return Task.FromResult(FromEntity(entity));
    }
}
