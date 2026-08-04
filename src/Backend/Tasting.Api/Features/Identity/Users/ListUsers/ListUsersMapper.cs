using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersMapper : BaseQueryMapper<ListUsersRequest, ListUsersResponse, ListUsersQuery, ListUsersResult>
{
    public override ListUsersQuery ToQuery(ListUsersRequest req)
    {
        return new ListUsersQuery(req.SearchTerm);
    }

    public override Task<ListUsersResponse> FromEntityAsync(ListUsersResult entity, CancellationToken ct = default)
    {
        var users = entity.Users
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

        return Task.FromResult(new ListUsersResponse { Users = users });
    }
}
