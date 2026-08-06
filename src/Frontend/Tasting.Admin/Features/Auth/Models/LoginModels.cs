using System.ComponentModel.DataAnnotations;

namespace Tasting.Admin.Features.Auth.Models;

public sealed record LoginRequest(string Email, string Password);

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = "";
}

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
