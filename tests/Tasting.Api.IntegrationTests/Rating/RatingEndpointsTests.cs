using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Tasting.Api.Contracts;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.Infrastructure.Rating;
using Tasting.Api.IntegrationTests.Infrastructure;

namespace Tasting.Api.IntegrationTests.Rating;

/// <summary>
/// Integration tests for SubmitRating and GetResults endpoints.
/// Uses WebApplicationFactory with in-memory DbContext and a self-signed JWT.
/// </summary>
public class RatingEndpointsTests : IClassFixture<RatingTestWebFactory>
{
    private readonly HttpClient _client;
    private readonly RatingTestWebFactory _factory;

    public RatingEndpointsTests(RatingTestWebFactory factory)
    {
        _factory = factory;
        _factory.ResetStubDefaults();
        _client = factory.CreateClient();
        _factory.EnsureUserSeededAsync().GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", RatingTestWebFactory.GenerateTestToken());
    }

    [Fact]
    public async Task SubmitRating_ValidRequest_Returns201()
    {
        var arrangementId = Guid.NewGuid();
        var payload = new
        {
            beerId = Guid.NewGuid(),
            visibility = 8.0,
            smell = 7.5,
            taste = 9.0,
            toast = 8.5
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(8.25m, body.GetProperty("totalRating").GetDecimal());
    }

    [Fact]
    public async Task SubmitRating_InvalidScore_Returns400()
    {
        var arrangementId = Guid.NewGuid();
        var payload = new { beerId = Guid.NewGuid(), visibility = 11.0, smell = 5.0, taste = 5.0, toast = 5.0 };

        var response = await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_error", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SubmitRating_NonStartedArrangement_Returns409()
    {
        _factory.ArrangementServiceStub
            .GetStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ArrangementStatus.Created);

        var arrangementId = Guid.NewGuid();
        var payload = new { beerId = Guid.NewGuid(), visibility = 5.0, smell = 5.0, taste = 5.0, toast = 5.0 };

        var response = await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetResults_EmptyArrangement_ReturnsEmptyList()
    {
        var arrangementId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/arrangements/{arrangementId}/results");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("results").GetArrayLength());
    }

    [Fact]
    public async Task SubmitRating_ThenGetResults_ReturnsRankedList()
    {
        var arrangementId = Guid.NewGuid();
        var beer1 = Guid.NewGuid();
        var beer2 = Guid.NewGuid();

        await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings",
            new { beerId = beer1, visibility = 9.0, smell = 9.0, taste = 9.0, toast = 9.0 });

        await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings",
            new { beerId = beer2, visibility = 6.0, smell = 6.0, taste = 6.0, toast = 6.0 });

        var response = await _client.GetAsync($"/api/v1/arrangements/{arrangementId}/results");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var results = body.GetProperty("results");
        Assert.Equal(2, results.GetArrayLength());
        Assert.Equal(1, results[0].GetProperty("rank").GetInt32());
        Assert.Equal(9.0m, results[0].GetProperty("totalRating").GetDecimal());
        Assert.Equal(2, results[1].GetProperty("rank").GetInt32());
        Assert.Equal(6.0m, results[1].GetProperty("totalRating").GetDecimal());
    }

    [Fact]
    public async Task SubmitRating_Upsert_Returns200OnSecondSubmit()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        var payload = new { beerId, visibility = 8.0, smell = 8.0, taste = 8.0, toast = 8.0 };

        await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload);
        var response = await _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrentFirstSubmissions_OneWins_OneConflicts_ResultStaysConsistent_AndFreshSubmitSucceeds()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        var payload = new { beerId, visibility = 8.0, smell = 8.0, taste = 8.0, toast = 8.0 };
        _factory.ConcurrentRatingReadBarrier.Arm();

        var responses = await Task.WhenAll(
            _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload),
            _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings", payload));

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        var loser = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        var error = await loser.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("conflict", error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("message").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("correlationId").GetString()));

        var resultsResponse = await _client.GetAsync($"/api/v1/arrangements/{arrangementId}/results");
        var results = (await resultsResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("results");
        Assert.Single(results.EnumerateArray());
        Assert.Equal(1, results[0].GetProperty("ratingCount").GetInt32());
        Assert.Equal(8.0m, results[0].GetProperty("totalRating").GetDecimal());

        _factory.ResetStubDefaults();
        var freshResponse = await _client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/ratings",
            new { beerId, visibility = 9.0, smell = 9.0, taste = 9.0, toast = 9.0 });
        Assert.Equal(HttpStatusCode.OK, freshResponse.StatusCode);
    }

    [Fact]
    public async Task ConcurrentUpdates_OneWins_OneConflicts_PreservesWinnerAndResult_AndFreshUpdateSucceeds()
    {
        var arrangementId = Guid.NewGuid();
        var beerId = Guid.NewGuid();
        await _client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/ratings",
            new { beerId, visibility = 5.0, smell = 5.0, taste = 5.0, toast = 5.0 });

        _factory.ConcurrentRatingReadBarrier.Arm();

        var responses = await Task.WhenAll(
            _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings",
                new { beerId, visibility = 7.0, smell = 7.0, taste = 7.0, toast = 7.0 }),
            _client.PostAsJsonAsync($"/api/v1/arrangements/{arrangementId}/ratings",
                new { beerId, visibility = 9.0, smell = 9.0, taste = 9.0, toast = 9.0 }));

        var winner = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        var loser = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        var winningRating = await winner.Content.ReadFromJsonAsync<JsonElement>();
        var winningTotal = winningRating.GetProperty("totalRating").GetDecimal();
        var error = await loser.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("conflict", error.GetProperty("code").GetString());

        var resultsResponse = await _client.GetAsync($"/api/v1/arrangements/{arrangementId}/results");
        var result = (await resultsResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("results")[0];
        Assert.Equal(1, result.GetProperty("ratingCount").GetInt32());
        Assert.Equal(winningTotal, result.GetProperty("totalRating").GetDecimal());

        _factory.ResetStubDefaults();
        var freshResponse = await _client.PostAsJsonAsync(
            $"/api/v1/arrangements/{arrangementId}/ratings",
            new { beerId, visibility = 6.0, smell = 6.0, taste = 6.0, toast = 6.0 });
        Assert.Equal(HttpStatusCode.OK, freshResponse.StatusCode);
    }
}

/// <summary>
/// WebApplicationFactory that:
/// - Replaces Npgsql with in-memory EF Core
/// - Overrides JWT validation to accept tokens signed by a test key
/// - Exposes a controllable IArrangementService stub
/// </summary>
public class RatingTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly Guid DefaultUserId = Guid.Parse("A7D3E5F1-1111-4444-8888-123456789ABC");
    private static readonly byte[] TestKeyBytes = Encoding.UTF8.GetBytes(
        "integration-test-secret-key-that-is-at-least-32-bytes");
    private readonly PostgresContainerFixture _postgres = new();
    private string? _previousConnectionString;

    public IArrangementService ArrangementServiceStub { get; } = Substitute.For<IArrangementService>();
    public ConcurrentRatingReadBarrier ConcurrentRatingReadBarrier { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TastingDb");
        Environment.SetEnvironmentVariable("ConnectionStrings__TastingDb", _postgres.ConnectionString);
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__TastingDb", _previousConnectionString);
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    public void ResetStubDefaults()
    {
        ArrangementServiceStub.GetStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ArrangementStatus.Started);
        ArrangementServiceStub.IsParticipantAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        ArrangementServiceStub.IsBeerInArrangementAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        ArrangementServiceStub.GetBeerNameSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("Test Beer");
        ArrangementServiceStub.GetParticipantNameSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("Test User");
    }

    public static string GenerateTestToken(string userId = "A7D3E5F1-1111-4444-8888-123456789ABC")
    {
        var key = new SymmetricSecurityKey(TestKeyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim("sub", userId)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task EnsureUserSeededAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        if (await db.Users.AnyAsync(user => user.Id == DefaultUserId))
        {
            return;
        }

        db.Users.Add(new User
        {
            Id = DefaultUserId,
            Email = "rating.user@test.no",
            EmailNormalized = "rating.user@test.no",
            FirstName = "Rating",
            LastName = "User",
            IsActive = true,
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TastingDb"] = _postgres.ConnectionString
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IArrangementService>();
            services.AddSingleton(ArrangementServiceStub);
            services.RemoveAll<RatingDbContext>();
            services.RemoveAll<DbContextOptions<RatingDbContext>>();
            services.AddSingleton(ConcurrentRatingReadBarrier);
            services.AddDbContext<RatingDbContext>((provider, options) => options
                .UseNpgsql(_postgres.ConnectionString)
                .AddInterceptors(provider.GetRequiredService<ConcurrentRatingReadBarrier>()));

            // Override JWT validation without changing scheme registration
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.Authority = null;
                options.MetadataAddress = null;
                options.Backchannel = new HttpClient();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    IssuerSigningKey = new SymmetricSecurityKey(TestKeyBytes)
                };
            });
        });
    }
}

public sealed class ConcurrentRatingReadBarrier : DbCommandInterceptor
{
    private TaskCompletionSource? _bothReadsStarted;
    private int _remainingReads;

    public void Arm()
    {
        _remainingReads = 2;
        _bothReadsStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        var gate = _bothReadsStarted;
        if (gate is null
            || Volatile.Read(ref _remainingReads) <= 0
            || !command.CommandText.Contains("ratings", StringComparison.OrdinalIgnoreCase))
            return result;

        if (Interlocked.Decrement(ref _remainingReads) == 0)
            gate.TrySetResult();

        await gate.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        _bothReadsStarted = null;
        return result;
    }
}
