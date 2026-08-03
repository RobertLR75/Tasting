using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersEndpoint(IRequestHandler<ListBeersQuery, ListBeersResult> handler)
    : BaseQueryEndpoint<ListBeersRequest, ListBeersResponse, ListBeersQuery, ListBeersResult, ListBeersMapper>(handler)
{
    public override void Configure()
    {
        Get("/beers");
    }

    public override async Task HandleAsync(ListBeersRequest req, CancellationToken ct)
    {
        var isAdmin = HttpContext.User?.IsInRole("Admin") == true;
        if (req.IncludeInactive && !isAdmin)
        {
            throw new ForbiddenException("Only admins can include inactive beers.");
        }

        await base.HandleAsync(req, ct);
    }
}
