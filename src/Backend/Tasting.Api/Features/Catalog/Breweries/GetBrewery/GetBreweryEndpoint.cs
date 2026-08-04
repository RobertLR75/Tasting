using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.GetBrewery;

public sealed class GetBreweryEndpoint(IRequestHandler<GetBreweryQuery, BreweryResponse> handler)
    : BaseQueryEndpoint<GetBreweryRequest, BreweryResponse, GetBreweryQuery, BreweryResponse, GetBreweryMapper>(handler)
{
    public override void Configure()
    {
        Get("/breweries/{id:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
