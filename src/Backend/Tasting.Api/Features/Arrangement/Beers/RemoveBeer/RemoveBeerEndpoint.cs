using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Beers.RemoveBeer;

public sealed class RemoveBeerEndpoint(
    IRequestHandler<RemoveBeerCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        RemoveBeerRequest,
        ArrangementResponse,
        RemoveBeerCommand,
        Domain.Arrangement,
        RemoveBeerMapper>(handler)
{
    public override void Configure()
    {
        Delete("/arrangements/{arrangementId:guid}/beers/{beerId:guid}");
        Description(d => d.WithTags("Arrangement Beers"));
        Roles(UserRole.Admin.ToString());
    }

    protected override RemoveBeerCommand ToCommand(RemoveBeerRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        var beerId = Route<Guid>("beerId");
        return new RemoveBeerCommand(arrangementId, beerId, req.RowVersion);
    }

    protected override async Task HandleResponseAsync(ArrangementResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
