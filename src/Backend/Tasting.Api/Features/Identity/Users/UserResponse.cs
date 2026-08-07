namespace Tasting.Api.Features.Identity.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
