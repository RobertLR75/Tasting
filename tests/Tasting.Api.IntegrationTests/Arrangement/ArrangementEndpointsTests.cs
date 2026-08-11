using System.Net;
using System.Net.Http.Json;
using SharedLibrary.FastEndpoints.Contracts;
using Tasting.Api.Features.Arrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;
using Xunit;
using ArrangementEntity = Tasting.Api.Infrastructure.Arrangement.ArrangementRecord;

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
            new { userId });

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
            new { userId });

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
            new { beerId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Activate_Then_Start_Succeeds()
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

        var activateResponse = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/activate",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Created, activateResponse.StatusCode);
        var activated = await activateResponse.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(activated);
        Assert.Equal(ArrangementStatus.Active, activated.Status);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/start",
            new { });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(ArrangementStatus.Started, body.Status);
    }

    [Fact]
    public async Task Start_Without_Activate_Returns409()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Created only",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/start",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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

    // ── GetArrangement ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetArrangement_ReturnsOk_WhenExists()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Get Test",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.GetAsync($"/api/v1/arrangements/{arrangementId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(arrangementId, body.Id);
        Assert.Equal("Get Test", body.Name);
    }

    [Fact]
    public async Task GetArrangement_ReturnsNotFound_WhenMissing()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.GetAsync($"/api/v1/arrangements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetArrangement_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/arrangements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── ListArrangements ────────────────────────────────────────────────────

    [Fact]
    public async Task ListArrangements_ReturnsOk_WithItems()
    {
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = Guid.NewGuid(),
                Name = "List Test",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.GetAsync("/api/v1/arrangements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── UpdateArrangement ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateArrangement_ReturnsOk_WhenCreated()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Old Name",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}",
            new { name = "New Name", description = (string?)null, rowVersion = 0 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal("New Name", body.Name);
    }

    [Fact]
    public async Task UpdateArrangement_IgnoresLegacyRowVersionInput()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Old Name",
                Status = ArrangementStatus.Created,
                RowVersion = 2,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}",
            new { name = "New Name", description = (string?)null, rowVersion = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateArrangement_SucceedsWithoutRowVersion_WhenCurrentVersionIsNotZero()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Old Name",
                Status = ArrangementStatus.Created,
                RowVersion = 1,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PutAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}",
            new { name = "New Name", description = (string?)null });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateArrangement_ReturnsConflict_WhenNotCreated()
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

        var response = await client.PutAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}",
            new { name = "X", description = (string?)null, rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── CancelArrangement ───────────────────────────────────────────────────

    [Fact]
    public async Task CancelArrangement_TransitionsToCanceled()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "To Cancel",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/cancel",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(ArrangementStatus.Canceled, body.Status);
    }

    [Fact]
    public async Task CancelArrangement_ReturnsConflict_WhenStarted()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Already Started",
                Status = ArrangementStatus.Started,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/cancel",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── ReopenArrangement ───────────────────────────────────────────────────

    [Fact]
    public async Task ReopenArrangement_TransitionsToCreated_WhenCanceled()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "To Reopen",
                Status = ArrangementStatus.Canceled,
                RowVersion = 4,
                CreatedAt = DateTimeOffset.UtcNow
            };
            arrangement.Beers.Add(new ArrangementBeer
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangementId,
                BeerId = beerId,
                NameSnapshot = "Preserved Beer",
                CreatedAt = DateTimeOffset.UtcNow
            });
            arrangement.Participants.Add(new ArrangementParticipant
            {
                Id = Guid.NewGuid(),
                ArrangementId = arrangementId,
                UserId = userId,
                FirstNameSnapshot = "Ada",
                LastNameSnapshot = "Admin",
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.Arrangements.Add(arrangement);
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/reopen",
            new { rowVersion = 4 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(ArrangementStatus.Created, body.Status);
        Assert.NotNull(body.UpdatedAt);
        Assert.Single(body.Beers);
        Assert.Equal(beerId, body.Beers[0].BeerId);
        Assert.Single(body.Participants);
        Assert.Equal(userId, body.Participants[0].UserId);
    }

    [Fact]
    public async Task ReopenArrangement_ReturnsConflict_WhenCreated()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Already Created",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/reopen",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── CompleteArrangement ─────────────────────────────────────────────────

    [Fact]
    public async Task CompleteArrangement_TransitionsToCompleted()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "To Complete",
                Status = ArrangementStatus.Started,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/complete",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ArrangementResponse>();
        Assert.NotNull(body);
        Assert.Equal(ArrangementStatus.Completed, body.Status);
    }

    [Fact]
    public async Task CompleteArrangement_ReturnsConflict_WhenCreated()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db =>
        {
            db.Arrangements.Add(new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Not Started",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/complete",
            new { rowVersion = 0 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── RemoveParticipant ───────────────────────────────────────────────────

    [Fact]
    public async Task RemoveParticipant_ReturnsOk_WhenCreated()
    {
        var arrangementId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "With Participant",
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

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/arrangements/{arrangementId}/participants/{userId}");
        request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemoveParticipant_ReturnsConflict_WhenNotCreated()
    {
        var arrangementId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Started",
                Status = ArrangementStatus.Started,
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

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin");

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/arrangements/{arrangementId}/participants/{userId}");
        request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── RemoveBeer ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveBeer_ReturnsOk_WhenCreated()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "With Beer",
                Status = ArrangementStatus.Created,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };
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

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/arrangements/{arrangementId}/beers/{beerId}");
        request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantDiscovery_ReturnsOnlyActiveArrangements()
    {
        var activeId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db => db.Arrangements.AddRange(
            new ArrangementEntity { Id = activeId, Name = "Visible", Status = ArrangementStatus.Active, CreatedAt = DateTimeOffset.UtcNow },
            new ArrangementEntity { Id = Guid.NewGuid(), Name = "Hidden", Status = ArrangementStatus.Created, CreatedAt = DateTimeOffset.UtcNow }));

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.GetAsync("/api/v1/participant/arrangements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<
            Tasting.Api.Features.Arrangement.Participants.ListVisibleArrangements.ListVisibleArrangementsResponse>();
        Assert.NotNull(body);
        Assert.Contains(body.Items, item => item.Id == activeId);
        Assert.DoesNotContain(body.Items, item => item.Name == "Hidden");
    }

    [Fact]
    public async Task ParticipantSelfJoin_UsesAuthenticatedUser_AndRejectsDuplicate()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db => db.Arrangements.Add(new ArrangementEntity
        {
            Id = arrangementId, Name = "Joinable", Status = ArrangementStatus.Active, CreatedAt = DateTimeOffset.UtcNow
        }));

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var joined = await client.PostAsync($"/api/v1/participant/arrangements/{arrangementId}/join", null);
        var duplicate = await client.PostAsync($"/api/v1/participant/arrangements/{arrangementId}/join", null);

        Assert.Equal(HttpStatusCode.OK, joined.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var error = await duplicate.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("conflict", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
    }

    [Fact]
    public async Task ParticipantSelfJoin_ReturnsUnifiedNotFoundError_WhenArrangementDoesNotExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.PostAsync($"/api/v1/participant/arrangements/{Guid.NewGuid()}/join", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("not_found", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
    }

    [Fact]
    public async Task ParticipantSelfJoin_ReturnsUnifiedConflictError_WhenArrangementIsNotActive()
    {
        var arrangementId = Guid.NewGuid();
        await _factory.SeedArrangementAsync(db => db.Arrangements.Add(new ArrangementEntity
        {
            Id = arrangementId, Name = "Not active", Status = ArrangementStatus.Created, CreatedAt = DateTimeOffset.UtcNow
        }));
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user");

        var response = await client.PostAsync($"/api/v1/participant/arrangements/{arrangementId}/join", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("conflict", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
    }

    [Fact]
    public async Task ParticipantDiscovery_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/participant/arrangements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveBeer_ReturnsConflict_WhenNotCreated()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();

        await _factory.SeedArrangementAsync(db =>
        {
            var arrangement = new ArrangementEntity
            {
                Id = arrangementId,
                Name = "Started",
                Status = ArrangementStatus.Started,
                RowVersion = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };
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

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/arrangements/{arrangementId}/beers/{beerId}");
        request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
