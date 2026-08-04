using System.Net;
using System.Net.Http.Json;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.IntegrationTests.Identity;

public sealed class IdentityEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;

    public IdentityEndpointsTests(IdentityApiFactory factory)
    {
        _client = factory.CreateClient();
        factory.EnsureSeededAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Create_user_returns_created_for_authenticated_admin()
    {
        var request = new
        {
            Email = "new-user@tasting.no",
            FirstName = "New",
            LastName = "User",
            Role = UserRole.User
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_user_returns_ok_for_authenticated_user()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{IdentityApiFactory.UserId}");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.UserId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.User.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_user_requires_admin()
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/v1/users/{IdentityApiFactory.UserId}/deactivate");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.UserId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.User.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
