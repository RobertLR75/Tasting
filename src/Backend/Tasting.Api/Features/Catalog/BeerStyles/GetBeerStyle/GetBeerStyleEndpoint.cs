using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;

public sealed class GetBeerStyleEndpoint(IRequestHandler<GetBeerStyleQuery, BeerStyleResponse> handler)
    : BaseQueryEndpoint<GetBeerStyleRequest, BeerStyleResponse, GetBeerStyleQuery, BeerStyleResponse, GetBeerStyleMapper>(handler)
{
    public override void Configure()
    {
        Get("/beer-styles/{id:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
