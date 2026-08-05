using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.Features.Auth.Services;

public sealed class TastingAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private AuthenticationState _current = Anonymous;
    private string? _token;

    public string? Token => _token;

    public void NotifyLogin(LoginResponse response)
    {
        _token = response.Token;
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, $"{response.FirstName} {response.LastName}"),
            new Claim(ClaimTypes.Email, response.Email),
            new Claim(ClaimTypes.Role, response.Role),
        ], authenticationType: "jwt");

        _current = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public void NotifyLogout()
    {
        _token = null;
        _current = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(_current);
}
