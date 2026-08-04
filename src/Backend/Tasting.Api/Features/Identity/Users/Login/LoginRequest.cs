namespace Tasting.Api.Features.Identity.Users.Login;

public sealed record LoginRequest(
    string Email,
    string Password
);

public sealed record LoginResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role
);
