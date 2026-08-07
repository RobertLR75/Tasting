using System.Net;
using System.Text.Json;
using Tasting.Admin.Features.Arrangement.Models;
using Tasting.Admin.Features.Arrangement.Services;
using Tasting.Admin.Features.Auth.Models;
using Tasting.Admin.Features.Auth.Services;

namespace Tasting.Admin.UnitTests;

public sealed class HttpClientAuthorizationTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task AuthorizationHandler_SendsBearerTokenAndInvalidatesSessionOnAuthFailure(HttpStatusCode statusCode)
    {
        var innerHandler = new CapturingHandler(statusCode);
        var sessionStore = new InMemoryAdminSessionStore();
        var authState = new TastingAuthStateProvider(sessionStore);
        await authState.NotifyLoginAsync(new LoginResponse(
            Token: "test-token",
            Email: "admin@example.test",
            FirstName: "Ada",
            LastName: "Admin",
            Role: "Admin"));
        var authHandler = new AuthorizationMessageHandler(authState)
        {
            InnerHandler = innerHandler
        };
        var httpClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var client = new ArrangementsApiClient(httpClient);

        if (statusCode == HttpStatusCode.OK)
        {
            await client.ListAsync();
        }
        else
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => client.ListAsync());
        }

        Assert.Equal(HttpMethod.Get, innerHandler.Request?.Method);
        Assert.Equal("/api/v1/arrangements", innerHandler.Request?.RequestUri?.AbsolutePath);
        Assert.Equal("Bearer", innerHandler.Request?.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", innerHandler.Request?.Headers.Authorization?.Parameter);

        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Assert.Null(await sessionStore.LoadAsync());
        }
    }

    [Fact]
    public void AuthorizationHandler_ShouldNotNavigateFromHttpPipeline()
    {
        var source = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Auth/Services/AuthorizationMessageHandler.cs"));

        Assert.DoesNotContain("NavigationManager", source);
        Assert.DoesNotContain("NavigateTo", source);
    }

    [Fact]
    public async Task AuthStateProvider_RestoresAuthenticationStateFromStoredAdminSession()
    {
        var sessionStore = new InMemoryAdminSessionStore
        {
            Session = new StoredAdminSession(
                Token: "stored-token",
                Email: "admin@example.test",
                FirstName: "Ada",
                LastName: "Admin",
                Role: "Admin")
        };
        var authState = new TastingAuthStateProvider(sessionStore);

        var state = await authState.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("Ada Admin", state.User.Identity?.Name);
        Assert.Equal("stored-token", await authState.GetTokenAsync());
    }

    [Fact]
    public async Task ArrangementsApiClient_UpdateAsync_SendsRowVersion()
    {
        var innerHandler = new CapturingHandler(HttpStatusCode.OK)
        {
            ResponseContent = """{"id":"2771b182-209c-4372-a4fa-101c186e15c1","name":"Updated","description":null,"status":0,"rowVersion":8,"createdAt":"2026-08-06T00:00:00Z","updatedAt":null}"""
        };
        var httpClient = new HttpClient(innerHandler)
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var client = new ArrangementsApiClient(httpClient);
        var arrangementId = Guid.Parse("2771b182-209c-4372-a4fa-101c186e15c1");

        await client.UpdateAsync(arrangementId, new UpdateArrangementRequest("Updated", null, 7));

        Assert.Equal(HttpMethod.Put, innerHandler.Request?.Method);
        Assert.Equal($"/api/v1/arrangements/{arrangementId}", innerHandler.Request?.RequestUri?.AbsolutePath);
        using var document = JsonDocument.Parse(innerHandler.RequestBody ?? "{}");
        Assert.Equal(7, document.RootElement.GetProperty("rowVersion").GetInt32());
    }

    [Fact]
    public async Task ArrangementsApiClient_GetAsync_ReadsRowVersion()
    {
        var innerHandler = new CapturingHandler(HttpStatusCode.OK)
        {
            ResponseContent = """{"id":"2771b182-209c-4372-a4fa-101c186e15c1","name":"Existing","description":null,"status":0,"rowVersion":7,"createdAt":"2026-08-06T00:00:00Z","updatedAt":null}"""
        };
        var httpClient = new HttpClient(innerHandler)
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var client = new ArrangementsApiClient(httpClient);

        var arrangement = await client.GetAsync(Guid.Parse("2771b182-209c-4372-a4fa-101c186e15c1"));

        Assert.NotNull(arrangement);
        Assert.Equal(7u, arrangement.RowVersion);
    }

    [Fact]
    public async Task ArrangementsApiClient_ReopenAsync_PostsRowVersionToReopenEndpoint()
    {
        var innerHandler = new CapturingHandler(HttpStatusCode.OK)
        {
            ResponseContent = """{"id":"2771b182-209c-4372-a4fa-101c186e15c1","name":"Reopened","description":null,"status":0,"rowVersion":8,"createdAt":"2026-08-06T00:00:00Z","updatedAt":null}"""
        };
        var httpClient = new HttpClient(innerHandler)
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var client = new ArrangementsApiClient(httpClient);
        var arrangementId = Guid.Parse("2771b182-209c-4372-a4fa-101c186e15c1");

        await client.ReopenAsync(arrangementId, 7);

        Assert.Equal(HttpMethod.Post, innerHandler.Request?.Method);
        Assert.Equal($"/api/v1/arrangements/{arrangementId}/reopen", innerHandler.Request?.RequestUri?.AbsolutePath);
        using var document = JsonDocument.Parse(innerHandler.RequestBody ?? "{}");
        Assert.Equal(7, document.RootElement.GetProperty("rowVersion").GetInt32());
    }

    private sealed class CapturingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }
        public string ResponseContent { get; set; } = """{"items":[]}""";

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(ResponseContent)
            });
        }
    }

    private sealed class InMemoryAdminSessionStore : IAdminSessionStore
    {
        public StoredAdminSession? Session { get; set; }

        public Task<StoredAdminSession?> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Session);

        public Task SaveAsync(StoredAdminSession session, CancellationToken cancellationToken = default)
        {
            Session = session;
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Session = null;
            return Task.CompletedTask;
        }
    }

    private static string GetProjectFile(string relativePath)
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
}
