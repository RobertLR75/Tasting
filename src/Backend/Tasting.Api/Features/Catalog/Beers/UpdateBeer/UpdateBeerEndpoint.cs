using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Beers.UpdateBeer;

public sealed class UpdateBeerEndpoint(IRequestHandler<UpdateBeerCommand, Beer> handler)
    : BaseCommandEndpoint<UpdateBeerRequest, BeerResponse, UpdateBeerCommand, Beer, UpdateBeerMapper>(handler)
{
    public override void Configure()
    {
        Put("/beers/{id:guid}");
        Description(d => d.WithTags("Beers"));
        Roles(UserRole.Admin.ToString());
    }

    protected override UpdateBeerCommand ToCommand(UpdateBeerRequest req)
    {
        var id = Route<Guid>("id");
        return new UpdateBeerCommand(id, req.BreweryId, req.BeerStyleId, req.BeerTypeId, req.Name, req.IsActive);
    }

    protected override Task HandleResponseAsync(BeerResponse response, CancellationToken ct)
        => Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
}
