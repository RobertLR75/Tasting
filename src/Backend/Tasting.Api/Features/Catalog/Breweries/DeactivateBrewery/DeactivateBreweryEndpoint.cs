using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;

public sealed class DeactivateBreweryEndpoint(IRequestHandler<DeactivateBreweryCommand, Brewery> handler)
    : BaseCommandEndpoint<DeactivateBreweryRequest, BreweryResponse, DeactivateBreweryCommand, Brewery, DeactivateBreweryMapper>(handler)
{
    public override void Configure()
    {
        Patch("/breweries/{id:guid}/deactivate");
        Description(d => d.WithTags("Breweries"));
        Roles(UserRole.Admin.ToString());
    }

    protected override Task HandleResponseAsync(BreweryResponse response, CancellationToken ct)
        => Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
}
