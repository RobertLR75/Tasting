using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Beers.GetBeer;

public sealed class GetBeerEndpoint(IRequestHandler<GetBeerQuery, BeerResponse> handler)
    : BaseQueryEndpoint<GetBeerRequest, BeerResponse, GetBeerQuery, BeerResponse, GetBeerMapper>(handler)
{
    public override void Configure()
    {
        Get("/beers/{id:guid}");
        Description(d => d.WithTags("Beers"));
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
