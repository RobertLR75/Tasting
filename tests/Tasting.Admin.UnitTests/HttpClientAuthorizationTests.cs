using System.Net;
using Tasting.Admin.Features.Arrangement.Services;
using Tasting.Admin.Features.Auth.Models;
using Tasting.Admin.Features.Auth.Services;

namespace Tasting.Admin.UnitTests;

public sealed class HttpClientAuthorizationTests
{
    [Theory]
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

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.ListAsync());

        Assert.Equal(HttpMethod.Get, innerHandler.Request?.Method);
        Assert.Equal("/api/v1/arrangements", innerHandler.Request?.RequestUri?.AbsolutePath);
        Assert.Equal("Bearer", innerHandler.Request?.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", innerHandler.Request?.Headers.Authorization?.Parameter);
        Assert.Contains(((int)statusCode).ToString(), exception.Message);
        var inner = Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Equal(statusCode, inner.StatusCode);
        Assert.Null(await sessionStore.LoadAsync());
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

    private sealed class CapturingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                RequestMessage = request
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
