using FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Arrangement.Participants.ListVisibleArrangements;

public sealed class ListVisibleArrangementsEndpoint(
    IRequestHandler<ListVisibleArrangementsQuery, ListVisibleArrangementsResponse> handler)
    : EndpointWithoutRequest<ListVisibleArrangementsResponse>
{
    public override void Configure()
    {
        Get("/participant/arrangements");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
        Description(description => description.WithTags("Participant arrangements"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Response = await handler.HandleAsync(new ListVisibleArrangementsQuery(User.GetUserId()), ct);
    }
}
