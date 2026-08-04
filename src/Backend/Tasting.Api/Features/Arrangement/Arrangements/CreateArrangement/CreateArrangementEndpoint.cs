using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;

public sealed class CreateArrangementEndpoint(
    IRequestHandler<CreateArrangementCommand, Domain.Arrangement> handler)
    : BaseCommandEndpoint<
        CreateArrangementRequest,
        ArrangementResponse,
        CreateArrangementCommand,
        Domain.Arrangement,
        CreateArrangementMapper>(handler)
{
    public override void Configure()
    {
        Post("/arrangements");
        Roles(UserRole.Admin.ToString());
    }

    protected override CreateArrangementCommand ToCommand(CreateArrangementRequest req)
        => new(req.Name, req.Description);
}
