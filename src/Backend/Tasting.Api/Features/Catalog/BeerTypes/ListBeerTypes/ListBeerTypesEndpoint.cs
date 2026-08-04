using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesEndpoint(IRequestHandler<ListBeerTypesQuery, ListBeerTypesResponse> handler)
    : BaseQueryEndpoint<ListBeerTypesRequest, ListBeerTypesResponse, ListBeerTypesQuery, ListBeerTypesResponse, ListBeerTypesMapper>(handler)
{
    public override void Configure()
    {
        Get("/beer-types");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
