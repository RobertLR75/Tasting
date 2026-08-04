using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;

public sealed class GetBeerTypeEndpoint(IRequestHandler<GetBeerTypeQuery, BeerTypeResponse> handler)
    : BaseQueryEndpoint<GetBeerTypeRequest, BeerTypeResponse, GetBeerTypeQuery, BeerTypeResponse, GetBeerTypeMapper>(handler)
{
    public override void Configure()
    {
        Get("/beer-types/{id:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
