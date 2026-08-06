using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.Features.Auth.Services;

public sealed class TastingAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly IAdminSessionStore? _sessionStore;
    private readonly TimeProvider _timeProvider;
    private AuthenticationState _current = Anonymous;
    private string? _token;
    private bool _hasLoadedSession;

    public string? Token => _token;

    public TastingAuthStateProvider()
    {
        _timeProvider = TimeProvider.System;
    }

    public TastingAuthStateProvider(IAdminSessionStore sessionStore, TimeProvider? timeProvider = null)
    {
        _sessionStore = sessionStore;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task NotifyLoginAsync(LoginResponse response, CancellationToken cancellationToken = default)
    {
        var session = StoredAdminSession.FromLoginResponse(response);
        if (_sessionStore is not null)
        {
            await _sessionStore.SaveAsync(session, cancellationToken);
        }

        SetSession(session);
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public void NotifyLogin(LoginResponse response)
    {
        SetSession(StoredAdminSession.FromLoginResponse(response));
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public async Task NotifyLogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionStore is not null)
        {
            await _sessionStore.ClearAsync(cancellationToken);
        }

        ClearSession();
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public void NotifyLogout()
    {
        ClearSession();
        NotifyAuthenticationStateChanged(Task.FromResult(_current));
    }

    public Task InvalidateSessionAsync(CancellationToken cancellationToken = default) =>
        NotifyLogoutAsync(cancellationToken);

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSessionLoadedAsync(cancellationToken);
        return _token;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureSessionLoadedAsync();
        return _current;
    }

    private async Task EnsureSessionLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_hasLoadedSession || _sessionStore is null)
        {
            return;
        }

        StoredAdminSession? session;
        try
        {
            session = await _sessionStore.LoadAsync(cancellationToken);
        }
        catch (AdminSessionStoreUnavailableException)
        {
            return;
        }

        _hasLoadedSession = true;
        if (session is null)
        {
            return;
        }

        if (IsExpired(session.Token))
        {
            await _sessionStore.ClearAsync(cancellationToken);
            return;
        }

        SetSession(session);
    }

    private void SetSession(StoredAdminSession session)
    {
        _token = session.Token;
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, $"{session.FirstName} {session.LastName}"),
            new Claim(ClaimTypes.Email, session.Email),
            new Claim(ClaimTypes.Role, session.Role),
        ], authenticationType: "jwt");

        _current = new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private bool IsExpired(string token)
    {
        var expiresAt = ReadJwtExpiration(token);
        return expiresAt is not null && expiresAt <= _timeProvider.GetUtcNow();
    }

    private static DateTimeOffset? ReadJwtExpiration(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payload = Base64UrlDecode(parts[1]);
            using var document = JsonDocument.Parse(payload);
            if (!document.RootElement.TryGetProperty("exp", out var expElement) ||
                !expElement.TryGetInt64(out var exp))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(exp);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    private void ClearSession()
    {
        _token = null;
        _current = Anonymous;
    }
}
