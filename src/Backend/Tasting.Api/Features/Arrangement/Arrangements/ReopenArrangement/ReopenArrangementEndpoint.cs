using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.ReopenArrangement;

public sealed class ReopenArrangementEndpoint(
    IRequestHandler<ReopenArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        ReopenArrangementRequest,
        ArrangementResponse,
        ReopenArrangementCommand,
        Domain.Arrangement,
        ReopenArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId:guid}/reopen");
        Description(d => d.WithTags("Arrangements"));
        Roles(UserRole.Admin.ToString());
    }

    protected override ReopenArrangementCommand ToCommand(ReopenArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new ReopenArrangementCommand(arrangementId);
    }
}
