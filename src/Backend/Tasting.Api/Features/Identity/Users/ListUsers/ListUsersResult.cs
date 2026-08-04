namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed record ListUsersResult(IReadOnlyCollection<User> Users);
