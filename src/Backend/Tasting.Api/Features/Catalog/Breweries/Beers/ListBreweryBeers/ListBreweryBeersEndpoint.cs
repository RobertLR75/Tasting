using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed class ListBreweryBeersEndpoint(IRequestHandler<ListBreweryBeersQuery, ListBreweryBeersResult> handler)
    : BaseQueryEndpoint<ListBreweryBeersRequest, ListBreweryBeersResponse, ListBreweryBeersQuery, ListBreweryBeersResult, ListBreweryBeersMapper>(handler)
{
    public override void Configure()
    {
        Get("/breweries/{breweryId:guid}/beers");
        Description(d => d.WithTags("Beers"));
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
