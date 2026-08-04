using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed class CreateBeerTypeMapper : BaseCommandMapper<CreateBeerTypeRequest, BeerTypeResponse, CreateBeerTypeCommand, BeerType>
{
    public override CreateBeerTypeCommand ToCommand(CreateBeerTypeRequest req) => new(req.Name);

    public override BeerTypeResponse FromEntity(BeerType entity)
        => new(entity.Id, entity.Name, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BeerTypeResponse> FromEntityAsync(BeerType entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
