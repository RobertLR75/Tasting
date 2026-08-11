using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.ActivateArrangement;

public sealed class ActivateArrangementEndpoint(
    IRequestHandler<ActivateArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        ActivateArrangementRequest,
        ArrangementResponse,
        ActivateArrangementCommand,
        Domain.Arrangement,
        ActivateArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId:guid}/activate");
        Description(d => d.WithTags("Arrangements"));
        Roles(UserRole.Admin.ToString());
    }

    protected override ActivateArrangementCommand ToCommand(ActivateArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new ActivateArrangementCommand(arrangementId);
    }
}
