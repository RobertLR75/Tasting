using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.IntegrationTests.Infrastructure;

public sealed class TastingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgres = new();
    private string? _previousConnectionString;

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
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build())
                .SetFallbackPolicy(new AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build());
        });
    }

    public async Task EnsureSystemUsersSeededAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        if (!await db.Users.AnyAsync(user => user.Id == TestAuthHandler.AdminUserId))
        {
            db.Users.Add(new User
            {
                Id = TestAuthHandler.AdminUserId,
                Email = "admin@test.no",
                EmailNormalized = "admin@test.no",
                FirstName = "Admin",
                LastName = "Test",
                IsActive = true,
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await db.Users.AnyAsync(user => user.Id == TestAuthHandler.RegularUserId))
        {
            db.Users.Add(new User
            {
                Id = TestAuthHandler.RegularUserId,
                Email = "user@test.no",
                EmailNormalized = "user@test.no",
                FirstName = "Regular",
                LastName = "Test",
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task SeedAsync(Action<CatalogDbContext> configure)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        configure(db);
        await db.SaveChangesAsync();
    }
}
