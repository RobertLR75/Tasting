using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;

public sealed class GetArrangementEndpoint(
    IRequestHandler<GetArrangementQuery, Domain.Arrangement> handler)
    : BaseQueryEndpoint<GetArrangementRequest, ArrangementResponse, GetArrangementQuery, Domain.Arrangement, GetArrangementMapper>(handler)
{
    public override void Configure()
    {
        Get("/arrangements/{arrangementId:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
