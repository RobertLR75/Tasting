using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesEndpoint(IRequestHandler<ListBeerStylesQuery, ListBeerStylesResponse> handler)
    : EndpointWithoutRequest<ListBeerStylesResponse>
{
    public override void Configure()
    {
        Get("/beer-styles");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await handler.HandleAsync(new ListBeerStylesQuery(), ct);
        await Send.ResponseAsync(result, StatusCodes.Status200OK, ct);
    }
}
