using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using Npgsql;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.IntegrationTests.Infrastructure;

namespace Tasting.Api.IntegrationTests.Identity;

public abstract class IdentityApiFactory(string provider) : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly PostgresContainerFixture _postgres = new();
    private string? _previousConnectionString;
    private string? _previousProvider;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TastingDb");
        _previousProvider = Environment.GetEnvironmentVariable("Persistence__Provider");
        Environment.SetEnvironmentVariable("ConnectionStrings__TastingDb", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("Persistence__Provider", provider);
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__TastingDb", _previousConnectionString);
        Environment.SetEnvironmentVariable("Persistence__Provider", _previousProvider);
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__TastingDb", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("Persistence__Provider", provider);
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TastingDb"] = _postgres.ConnectionString,
                ["Persistence:Provider"] = provider
            });
        });
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }

    public async Task EnsureSeededAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.ExecuteAsync("TRUNCATE TABLE users;");
        await InsertUserAsync(connection, new User
        {
            Id = AdminId, Email = "admin@tasting.no", EmailNormalized = "admin@tasting.no",
            FirstName = "Admin", LastName = "User", Role = UserRole.Admin, IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), CreatedAt = DateTimeOffset.UtcNow
        });
        await InsertUserAsync(connection, new User
        {
            Id = UserId, Email = "user@tasting.no", EmailNormalized = "user@tasting.no",
            FirstName = "Regular", LastName = "User", Role = UserRole.User, IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task SeedUserAsync(User user)
    {
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await InsertUserAsync(connection, user);
    }

    private static Task InsertUserAsync(NpgsqlConnection connection, User user)
        => connection.ExecuteAsync(
            """
            INSERT INTO users
                (id, email, email_normalized, first_name, last_name, password_hash, role, is_active, created_at_utc, updated_at_utc)
            VALUES
                (@Id, @Email, @EmailNormalized, @FirstName, @LastName, @PasswordHash, @Role, @IsActive, @CreatedAt, @UpdatedAt);
            """,
            new
            {
                user.Id, user.Email, user.EmailNormalized, user.FirstName, user.LastName, user.PasswordHash,
                Role = user.Role.ToString(), user.IsActive, user.CreatedAt, user.UpdatedAt
            });
}

public sealed class EntityFrameworkIdentityApiFactory() : IdentityApiFactory("EntityFramework");
public sealed class DapperIdentityApiFactory() : IdentityApiFactory("Dapper");
