using System.Net.Http.Json;
using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.Features.Auth.Services;

public interface IAuthApiClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/users/login", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (payload is null)
        {
            throw new InvalidOperationException("Login succeeded but the response payload was empty.");
        }

        if (!string.Equals(payload.Role, "Admin", StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Only administrators can access this application.");
        }

        return payload;
    }
}
