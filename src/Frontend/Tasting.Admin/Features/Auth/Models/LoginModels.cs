namespace Tasting.Admin.Features.Auth.Models;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role);

public sealed record StoredAdminSession(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role)
{
    public static StoredAdminSession FromLoginResponse(LoginResponse response) =>
        new(response.Token, response.Email, response.FirstName, response.LastName, response.Role);
}
