using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.IntegrationTests.Infrastructure;

public sealed class TastingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbSuffix = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.RemoveAll<CatalogDbContext>();
            services.RemoveAll<DbContextOptions<UsersDbContext>>();
            services.RemoveAll<UsersDbContext>();

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase($"catalog-int-{_dbSuffix}"));
            services.AddDbContext<UsersDbContext>(options =>
                options.UseInMemoryDatabase($"users-int-{_dbSuffix}"));

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
        if (await db.Users.AnyAsync())
        {
            return;
        }

        db.Users.AddRange(
            new User
            {
                Id = TestAuthHandler.AdminUserId,
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
                Id = TestAuthHandler.RegularUserId,
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

    public async Task SeedAsync(Action<CatalogDbContext> configure)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        configure(db);
        await db.SaveChangesAsync();
    }
}
