using System.Net;
using System.Net.Http.Headers;

namespace Tasting.Admin.Features.Auth.Services;

public sealed class AuthorizationMessageHandler(
    TastingAuthStateProvider authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await authState.GetTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (!IsLoginRequest(request) &&
            (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden))
        {
            await authState.InvalidateSessionAsync(cancellationToken);
        }

        return response;
    }

    private static bool IsLoginRequest(HttpRequestMessage request) =>
        request.RequestUri?.AbsolutePath.Equals("/api/v1/users/login", StringComparison.OrdinalIgnoreCase) is true;
}
