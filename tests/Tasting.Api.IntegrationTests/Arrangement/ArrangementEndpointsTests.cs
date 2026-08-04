using System.Net;
using System.Net.Http.Json;
using Tasting.Api.Features.Arrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.IntegrationTests.Arrangement;

public sealed class ArrangementEndpointsTests : IClassFixture<ArrangementApiFactory>, IAsyncLifetime
{
    private readonly ArrangementApiFactory _factory;

    public ArrangementEndpointsTests(ArrangementApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureSystemUsersSeededAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateArrangement_ReturnsForbidden_ForNonAdmin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.PostAsJsonAsync(
            "/api/v1/arrangements",
            new { name = "Summer Tasting", description = (string?)null });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateArrangement_ReturnsCreated_ForAdmin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            "/api/v1/arrangements",
            new { name = "Autumn Tasting", description = "Cozy autumn beers" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal("Autumn Tasting", body.Name);
        Assert.Equal(ArrangementStatus.Created, body.Status);
        Assert.Equal(0u, body.RowVersion);
    }

    [Fact]
    public async Task AddParticipant_ReturnsConflict_WhenArrangementAlreadyStarted()
    {
        var arrangementId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Started",
                Status = ArrangementStatus.Started,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/participants",
            new { userId, rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddParticipant_ReturnsConflict_WhenDuplicate()
    {
        var arrangementId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Test",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };
            arrangement.Participants.Add(new ArrangementParticipant
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangementId,
                UserId = userId,
                FirstNameSnapshot = string.Empty,
                LastNameSnapshot = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.Arrangements.Add(arrangement);
        });

        await _factory.SeedUsersAsync(db =>
        {
            db.Users.Add(new User
            {
                Id = userId,
                Email = "dup@example.com",
                EmailNormalized = "dup@example.com",
                FirstName = "Dup",
                LastName = "User",
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/participants",
            new { userId, rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddBeer_ReturnsConflict_WhenArrangementAlreadyStarted()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Started",
                Status = ArrangementStatus.Started,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/beers",
            new { beerId = Guid.NewGuid(), rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task StartArrangement_Transitions_CreatedToStarted_WithSnapshots()
    {
        var arrangementId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var breweryId = Guid.NewGuid();
        var styleId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var beerId = Guid.NewGuid();

        await _factory.SeedUsersAsync(db =>
        {
            db.Users.Add(new User
            {
                Id = userId,
                Email = "snap@example.com",
                EmailNormalized = "snap@example.com",
                FirstName = "Knut",
                LastName = "Hansen",
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        await _factory.SeedCatalogAsync(db =>
        {
            db.BeerStyles.Add(new BeerStyle { Id = styleId, Name = "Stout", CreatedAt = DateTimeOffset.UtcNow });
            db.BeerTypes.Add(new BeerType { Id = typeId, Name = "Dark", CreatedAt = DateTimeOffset.UtcNow });
            db.Breweries.Add(new Brewery { Id = breweryId, Name = "Snap Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
            db.Beers.Add(new Beer
            {
                Id = beerId,
                BreweryId = breweryId,
                BeerStyleId = styleId,
                BeerTypeId = typeId,
                Name = "Snap Stout",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Snap Test",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };
            arrangement.Participants.Add(new ArrangementParticipant
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangementId,
                UserId = userId,
                FirstNameSnapshot = string.Empty,
                LastNameSnapshot = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            });
            arrangement.Beers.Add(new ArrangementBeer
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangementId,
                BeerId = beerId,
                NameSnapshot = string.Empty,
                BreweryNameSnapshot = string.Empty,
                BeerStyleSnapshot = string.Empty,
                BeerTypeSnapshot = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.Arrangements.Add(arrangement);
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/start",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(ArrangementStatus.Started, body.Status);
    }

    [Fact]
    public async Task StartArrangement_ReturnsConflict_WhenAlreadyStarted()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Already started",
                Status = ArrangementStatus.Started,
                RowVersion = 2,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/start",
            new { rowVersion = 2 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
