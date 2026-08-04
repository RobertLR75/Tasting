using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed class CreateBeerStyleEndpoint(IRequestHandler<CreateBeerStyleCommand, BeerStyle> handler)
    : BaseCommandEndpoint<CreateBeerStyleRequest, BeerStyleResponse, CreateBeerStyleCommand, BeerStyle, CreateBeerStyleMapper>(handler)
{
    public override void Configure()
    {
        Post("/beer-styles");
        Roles(UserRole.Admin.ToString());
    }
}
