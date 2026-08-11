using FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Participants.SelfJoinArrangement;

public sealed class SelfJoinArrangementEndpoint(
    IRequestHandler<SelfJoinArrangementCommand, SelfJoinArrangementResponse> handler)
    : EndpointWithoutRequest<SelfJoinArrangementResponse>
{
    public override void Configure()
    {
        Post("/participant/arrangements/{arrangementId:guid}/join");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
        Description(description => description.WithTags("Participant arrangements"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new SelfJoinArrangementCommand(Route<Guid>("arrangementId"), User.GetUserId());
        Response = await handler.HandleAsync(command, ct);
    }
}
