using SharedLibrary.FastEndpoints;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed class CreateBeerStyleMapper : BaseCommandMapper<CreateBeerStyleRequest, BeerStyleResponse, CreateBeerStyleCommand, BeerStyle>
{
    public override CreateBeerStyleCommand ToCommand(CreateBeerStyleRequest req) => new(req.Name);

    public override BeerStyleResponse FromEntity(BeerStyle entity)
        => new(entity.Id, entity.Name, entity.CreatedAt, entity.UpdatedAt);

    public override Task<BeerStyleResponse> FromEntityAsync(BeerStyle entity, CancellationToken ct = default)
        => Task.FromResult(FromEntity(entity));
}
