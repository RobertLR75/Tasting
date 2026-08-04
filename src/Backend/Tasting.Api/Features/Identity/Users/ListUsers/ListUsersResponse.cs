namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersResponse
{
    public List<UserResponse> Users { get; init; } = [];
}
