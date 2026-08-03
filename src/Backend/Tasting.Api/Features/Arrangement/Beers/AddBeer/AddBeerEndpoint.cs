using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Beers.AddBeer;

public sealed class AddBeerEndpoint(
    IRequestHandler<AddBeerCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        AddBeerRequest,
        ArrangementResponse,
        AddBeerCommand,
        Domain.Arrangement,
        AddBeerMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId}/beers");
        Roles(UserRole.Admin.ToString());
    }

    protected override AddBeerCommand ToCommand(AddBeerRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new AddBeerCommand(arrangementId, req.BeerId, req.RowVersion);
    }
}
