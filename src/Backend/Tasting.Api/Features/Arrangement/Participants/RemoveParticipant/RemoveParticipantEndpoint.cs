using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Participants.RemoveParticipant;

public sealed class RemoveParticipantEndpoint(
    IRequestHandler<RemoveParticipantCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        RemoveParticipantRequest,
        ArrangementResponse,
        RemoveParticipantCommand,
        Domain.Arrangement,
        RemoveParticipantMapper>(handler)
{
    public override void Configure()
    {
        Delete("/arrangements/{arrangementId:guid}/participants/{userId:guid}");
        Description(d => d.WithTags("Participants"));
        Roles(UserRole.Admin.ToString());
    }

    protected override RemoveParticipantCommand ToCommand(RemoveParticipantRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        var userId = Route<Guid>("userId");
        return new RemoveParticipantCommand(arrangementId, userId, req.RowVersion);
    }

    protected override async Task HandleResponseAsync(ArrangementResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
