using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Participants.AddParticipant;

public sealed class AddParticipantEndpoint(
    IRequestHandler<AddParticipantCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        AddParticipantRequest,
        ArrangementResponse,
        AddParticipantCommand,
        Domain.Arrangement,
        AddParticipantMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements/{arrangementId}/participants");
        Description(d => d.WithTags("Participants"));
        Roles(UserRole.Admin.ToString());
    }

    protected override AddParticipantCommand ToCommand(AddParticipantRequest req)
    {
        var arrangementId = Route<Guid>("arrangementId");
        return new AddParticipantCommand(arrangementId, req.UserId);
    }
}
