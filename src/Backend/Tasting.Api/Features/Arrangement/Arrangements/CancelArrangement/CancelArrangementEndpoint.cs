using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;

public sealed class CancelArrangementEndpoint(
    IRequestHandler<CancelArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        CancelArrangementRequest,
        ArrangementResponse,
        CancelArrangementCommand,
        Domain.Arrangement,
        CancelArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId:guid}/cancel");
        Description(d => d.WithTags("Arrangements"));
        Roles(UserRole.Admin.ToString());
    }

    protected override CancelArrangementCommand ToCommand(CancelArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new CancelArrangementCommand(arrangementId);
    }
}
