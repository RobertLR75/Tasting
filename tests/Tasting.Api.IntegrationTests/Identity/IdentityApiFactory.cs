using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.IntegrationTests.Identity;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly string DatabaseName = $"tasting-api-integration-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<UsersDbContext>));
            services.RemoveAll(typeof(UsersDbContext));

            services.AddDbContext<UsersDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));

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
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        context.Users.Add(new User
        {
            Id = AdminId,
            Email = "admin@tasting.no",
            EmailNormalized = "admin@tasting.no",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        context.Users.Add(new User
        {
            Id = UserId,
            Email = "user@tasting.no",
            EmailNormalized = "user@tasting.no",
            FirstName = "Regular",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
