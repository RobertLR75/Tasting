using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;

public sealed class StartArrangementEndpoint(
    IRequestHandler<StartArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        StartArrangementRequest,
        ArrangementResponse,
        StartArrangementCommand,
        Domain.Arrangement,
        StartArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId}/start");
        Description(d => d.WithTags("Arrangements"));
        Roles(UserRole.Admin.ToString());
    }

    protected override StartArrangementCommand ToCommand(StartArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new StartArrangementCommand(arrangementId, req.RowVersion);
    }
}
