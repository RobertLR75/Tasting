using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesEndpoint(IRequestHandler<ListBeerStylesQuery, ListBeerStylesResponse> handler)
    : BaseQueryEndpoint<ListBeerStylesRequest, ListBeerStylesResponse, ListBeerStylesQuery, ListBeerStylesResponse, ListBeerStylesMapper>(handler)
{
    public override void Configure()
    {
        Get("/beer-styles");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
