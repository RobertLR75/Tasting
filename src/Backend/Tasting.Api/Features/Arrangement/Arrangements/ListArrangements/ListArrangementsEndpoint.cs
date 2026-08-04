using FastEndpoints;
using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed class ListArrangementsEndpoint(
    IRequestHandler<ListArrangementsQuery, ListArrangementsResult> handler)
    : BaseQueryEndpoint<ListArrangementsRequest, ListArrangementsResponse, ListArrangementsQuery, ListArrangementsResult, ListArrangementsMapper>(handler)
{
    public override void Configure()
    {
        Get("/arrangements");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
