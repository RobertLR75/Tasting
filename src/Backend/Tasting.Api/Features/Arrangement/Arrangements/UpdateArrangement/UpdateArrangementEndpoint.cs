using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;

public sealed class UpdateArrangementEndpoint(
    IRequestHandler<UpdateArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        UpdateArrangementRequest,
        ArrangementResponse,
        UpdateArrangementCommand,
        Domain.Arrangement,
        UpdateArrangementMapper>(handler)
{
    public override void Configure()
    {
        Put("/arrangements/{arrangementId:guid}");
        Roles(UserRole.Admin.ToString());
    }

    protected override UpdateArrangementCommand ToCommand(UpdateArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new UpdateArrangementCommand(arrangementId, req.Name, req.Description, req.RowVersion);
    }

    protected override async Task HandleResponseAsync(ArrangementResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
