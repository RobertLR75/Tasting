using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.IntegrationTests.Arrangement;

public sealed class ArrangementApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbSuffix = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ArrangementDbContext>>();
            services.RemoveAll<ArrangementDbContext>();
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.RemoveAll<CatalogDbContext>();
            services.RemoveAll<DbContextOptions<UsersDbContext>>();
            services.RemoveAll<UsersDbContext>();

            services.AddDbContext<ArrangementDbContext>(o =>
                o.UseInMemoryDatabase($"arrangement-int-{_dbSuffix}"));
            services.AddDbContext<CatalogDbContext>(o =>
                o.UseInMemoryDatabase($"catalog-int-{_dbSuffix}"));
            services.AddDbContext<UsersDbContext>(o =>
                o.UseInMemoryDatabase($"users-int-{_dbSuffix}"));

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
        if (await db.Users.AnyAsync())
        {
            return;
        }

        db.Users.AddRange(
            new User
            {
                Id = ArrangementTestAuthHandler.AdminUserId,
                Email = "admin@test.no",
                EmailNormalized = "admin@test.no",
                FirstName = "Admin",
                LastName = "Test",
                IsActive = true,
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new User
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
