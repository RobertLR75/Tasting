using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.IntegrationTests.Infrastructure;

namespace Tasting.Api.IntegrationTests.Arrangement;

public sealed class ArrangementApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
                    options.DefaultAuthenticateScheme = ArrangementTestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = ArrangementTestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, ArrangementTestAuthHandler>(
                    ArrangementTestAuthHandler.SchemeName, _ => { });

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder(ArrangementTestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build())
                .SetFallbackPolicy(new AuthorizationPolicyBuilder(ArrangementTestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build());
        });
    }

    public async Task EnsureSystemUsersSeededAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        if (!await db.Users.AnyAsync(user => user.Id == ArrangementTestAuthHandler.AdminUserId))
        {
            db.Users.Add(new User
            {
                Id = ArrangementTestAuthHandler.AdminUserId,
                Email = "admin@test.no",
                EmailNormalized = "admin@test.no",
                FirstName = "Admin",
                LastName = "Test",
                IsActive = true,
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await db.Users.AnyAsync(user => user.Id == ArrangementTestAuthHandler.RegularUserId))
        {
            db.Users.Add(new User
            {
                Id = ArrangementTestAuthHandler.RegularUserId,
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

    public async Task SeedArrangementAsync(Action<ArrangementDbContext> configure)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArrangementDbContext>();
        configure(db);
        await db.SaveChangesAsync();
    }

    public async Task SeedCatalogAsync(Action<CatalogDbContext> configure)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        configure(db);
        await db.SaveChangesAsync();
    }

    public async Task SeedUsersAsync(Action<UsersDbContext> configure)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        configure(db);
        await db.SaveChangesAsync();
    }
}
