using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using SharedLibrary.FastEndpoints.Contracts;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.Login;
using Tasting.Api.Features.Identity.Users.ListUsers;

namespace Tasting.Api.IntegrationTests.Identity;

public abstract class IdentityEndpointsTests
{
    private readonly HttpClient _client;
    private readonly IdentityApiFactory _factory;

    protected IdentityEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
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
            Password = "password123",
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
    public async Task Create_user_returns_conflict_for_case_insensitive_duplicate_email()
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users")
        {
            Content = JsonContent.Create(new
            {
                Email = "USER@TASTING.NO",
                FirstName = "Duplicate",
                LastName = "User",
                Password = "password123",
                Role = UserRole.User
            })
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_returns_token_for_active_admin_with_valid_password()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/users/login",
            new
            {
                email = "admin@tasting.no",
                password = "password123"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("admin@tasting.no", body.Email);
        Assert.Equal(UserRole.Admin.ToString(), body.Role);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body.Token);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == IdentityApiFactory.AdminId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == "role" && claim.Value == UserRole.Admin.ToString());
    }

    [Fact]
    public async Task Login_returns_token_for_active_participant_with_valid_password()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/users/login",
            new
            {
                email = "user@tasting.no",
                password = "password123"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("user@tasting.no", body.Email);
        Assert.Equal(UserRole.User.ToString(), body.Role);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body.Token);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == IdentityApiFactory.UserId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == "role" && claim.Value == UserRole.User.ToString());
    }

    [Fact]
    public async Task Login_returns_unified_unauthorized_error_for_invalid_credentials()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/users/login",
            new
            {
                email = "user@tasting.no",
                password = "wrong-password"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("unauthorized", body.Code);
        Assert.Equal("Invalid email or password.", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.CorrelationId));
    }

    [Fact]
    public async Task Get_user_returns_ok_for_authenticated_user()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{IdentityApiFactory.UserId}");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.UserId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.User.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(UserRole.User.ToString(), body.Role);
        Assert.Equal("Active", body.Status);
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

    [Fact]
    public async Task Deactivate_user_returns_ok_for_authenticated_admin()
    {
        var userId = Guid.NewGuid();
        await _factory.SeedUserAsync(new User
        {
            Id = userId,
            Email = "deactivate-me@tasting.no",
            EmailNormalized = "deactivate-me@tasting.no",
            FirstName = "Deactivate",
            LastName = "Me",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        using var message = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/v1/users/{userId}/deactivate")
        {
            Content = JsonContent.Create(new { id = userId })
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(userId, body.Id);
        Assert.Equal("Inactive", body.Status);
    }

    [Fact]
    public async Task List_users_returns_ok_for_authenticated_admin()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListUsersResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Users);
    }

    [Fact]
    public async Task List_users_supports_search_by_name_or_email()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users?searchTerm=admin");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ListUsersResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Users);
        var user = body.Users.Single();
        Assert.Equal(IdentityApiFactory.AdminId, user.Id);
        Assert.Contains("admin", user.Email, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task List_users_requires_admin()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users");
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.UserId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.User.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_user_returns_ok_for_authenticated_admin()
    {
        var request = new
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated-admin@tasting.no",
            Role = UserRole.Admin
        };

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{IdentityApiFactory.AdminId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_user_returns_conflict_when_email_taken()
    {
        var request = new
        {
            FirstName = "Regular",
            LastName = "User",
            Email = "user@tasting.no",
            Role = UserRole.User
        };

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{IdentityApiFactory.AdminId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_user_requires_admin()
    {
        var request = new
        {
            FirstName = "Regular",
            LastName = "User",
            Email = "user@tasting.no",
            Role = UserRole.User
        };

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{IdentityApiFactory.UserId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.UserId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.User.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "Valid", "User", "password123")]
    [InlineData("", "Valid", "User", "password123")]
    [InlineData("valid@test.no", "", "User", "password123")]
    [InlineData("valid@test.no", "Valid", "", "password123")]
    [InlineData("valid@test.no", "Valid", "User", "short")]
    public async Task Create_user_returns_bad_request_for_invalid_input(
        string email, string firstName, string lastName, string password)
    {
        var request = new { Email = email, FirstName = firstName, LastName = lastName, Password = password, Role = UserRole.User };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAuthHandler.UserIdHeader, IdentityApiFactory.AdminId.ToString());
        message.Headers.Add(TestAuthHandler.RoleHeader, UserRole.Admin.ToString());

        var response = await _client.SendAsync(message);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

[Collection("Identity provider matrix")]
public sealed class EntityFrameworkIdentityEndpointsTests(EntityFrameworkIdentityApiFactory factory)
    : IdentityEndpointsTests(factory), IClassFixture<EntityFrameworkIdentityApiFactory>;

[Collection("Identity provider matrix")]
public sealed class DapperIdentityEndpointsTests(DapperIdentityApiFactory factory)
    : IdentityEndpointsTests(factory), IClassFixture<DapperIdentityApiFactory>;

[CollectionDefinition("Identity provider matrix", DisableParallelization = true)]
public sealed class IdentityProviderMatrixCollection;
