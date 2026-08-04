using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesEndpoint(IRequestHandler<ListBeerTypesQuery, ListBeerTypesResponse> handler)
    : EndpointWithoutRequest<ListBeerTypesResponse>
{
    public override void Configure()
    {
        Get("/beer-types");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await handler.HandleAsync(new ListBeerTypesQuery(), ct);
        await Send.ResponseAsync(result, StatusCodes.Status200OK, ct);
    }
}
