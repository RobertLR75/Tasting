using System.Net;
using System.Text;
using Tasting.Admin.Features.Auth.Models;
using Tasting.Admin.Features.Auth.Services;

namespace Tasting.Admin.UnitTests;

public sealed class AuthApiClientTests
{
    [Fact]
    public async Task LoginAsync_RejectsNonAdminLoginResponse()
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"token":"token","email":"user@tasting.no","firstName":"Regular","lastName":"User","role":"User"}""",
                Encoding.UTF8,
                "application/json")
        });
        var client = new AuthApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") });

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            client.LoginAsync(new LoginRequest("user@tasting.no", "password123")));

        Assert.Equal("Only administrators can access this application.", exception.Message);
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response);
    }
}
