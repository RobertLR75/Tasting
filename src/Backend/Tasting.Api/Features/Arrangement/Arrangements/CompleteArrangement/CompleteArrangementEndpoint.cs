using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.CompleteArrangement;

public sealed class CompleteArrangementEndpoint(
    IRequestHandler<CompleteArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        CompleteArrangementRequest,
        ArrangementResponse,
        CompleteArrangementCommand,
        Domain.Arrangement,
        CompleteArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId:guid}/complete");
        Description(d => d.WithTags("Arrangements"));
        Roles(UserRole.Admin.ToString());
    }

    protected override CompleteArrangementCommand ToCommand(CompleteArrangementRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new CompleteArrangementCommand(arrangementId);
    }
}
