using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersEndpoint(IRequestHandler<ListBeersQuery, ListBeersResult> handler)
    : BaseQueryEndpoint<ListBeersRequest, ListBeersResponse, ListBeersQuery, ListBeersResult, ListBeersMapper>(handler)
{
    public override void Configure()
    {
        Get("/beers");
        Description(d => d.WithTags("Beers"));
    }

    public override async Task HandleAsync(ListBeersRequest req, CancellationToken ct)
    {
        var isAdmin = HttpContext.User?.IsInRole(UserRole.Admin.ToString()) == true;
        if (req.IncludeInactive && !isAdmin)
        {
            throw new ForbiddenException("Only admins can include inactive beers.");
        }

        await base.HandleAsync(req, ct);
    }
}
