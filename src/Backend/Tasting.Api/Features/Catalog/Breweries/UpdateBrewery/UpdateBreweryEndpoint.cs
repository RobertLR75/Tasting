using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed class UpdateBreweryEndpoint(IRequestHandler<UpdateBreweryCommand, Brewery> handler)
    : BaseCommandEndpoint<UpdateBreweryRequest, BreweryResponse, UpdateBreweryCommand, Brewery, UpdateBreweryMapper>(handler)
{
    public override void Configure()
    {
        Put("/breweries/{id:guid}");
        Roles(UserRole.Admin.ToString());
    }

    protected override Task HandleResponseAsync(BreweryResponse response, CancellationToken ct)
        => Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
}
