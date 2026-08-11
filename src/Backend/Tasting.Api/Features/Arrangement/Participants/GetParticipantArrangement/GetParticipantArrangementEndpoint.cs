using FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Participants.GetParticipantArrangement;

public sealed class GetParticipantArrangementEndpoint(
    IRequestHandler<GetParticipantArrangementQuery, ParticipantArrangementResponse> handler)
    : EndpointWithoutRequest<ParticipantArrangementResponse>
{
    public override void Configure()
    {
        Get("/participant/arrangements/{arrangementId:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
        Description(description => description.WithTags("Participant arrangements"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Response = await handler.HandleAsync(
            new GetParticipantArrangementQuery(Route<Guid>("arrangementId"), User.GetUserId()), ct);
    }
}
