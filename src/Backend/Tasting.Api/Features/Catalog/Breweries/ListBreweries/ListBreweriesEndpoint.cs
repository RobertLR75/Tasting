using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed class ListBreweriesEndpoint(IRequestHandler<ListBreweriesQuery, ListBreweriesResponse> handler)
    : BaseQueryEndpoint<ListBreweriesRequest, ListBreweriesResponse, ListBreweriesQuery, ListBreweriesResponse, ListBreweriesMapper>(handler)
{
    public override void Configure()
    {
        Get("/breweries");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(ListBreweriesRequest req, CancellationToken ct)
    {
        var isAdmin = HttpContext.User?.IsInRole(UserRole.Admin.ToString()) == true;
        if (req.IncludeInactive && !isAdmin)
        {
            throw new ForbiddenException("Only admins can include inactive breweries.");
        }

        await base.HandleAsync(req, ct);
    }
}
