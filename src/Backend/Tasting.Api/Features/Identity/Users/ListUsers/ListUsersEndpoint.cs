using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersEndpoint(IRequestHandler<ListUsersQuery, ListUsersResult> handler)
    : EndpointWithoutRequest<ListUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
        Description(d => d.WithTags("Users"));
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await handler.HandleAsync(new ListUsersQuery(), ct);

        var users = result.Users
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.Role,
                u.CreatedAt,
                u.UpdatedAt))
            .ToList();

        await Send.ResponseAsync(new ListUsersResponse { Users = users }, StatusCodes.Status200OK, ct);
    }
}
