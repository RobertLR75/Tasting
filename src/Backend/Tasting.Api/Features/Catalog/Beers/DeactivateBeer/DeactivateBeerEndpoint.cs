using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Beers.DeactivateBeer;

public sealed class DeactivateBeerEndpoint(IRequestHandler<DeactivateBeerCommand, Beer> handler)
    : BaseCommandEndpoint<DeactivateBeerRequest, BeerResponse, DeactivateBeerCommand, Beer, DeactivateBeerMapper>(handler)
{
    public override void Configure()
    {
        Patch("/beers/{id:guid}/deactivate");
        Roles(UserRole.Admin.ToString());
    }

    protected override Task HandleResponseAsync(BeerResponse response, CancellationToken ct)
        => Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
}
