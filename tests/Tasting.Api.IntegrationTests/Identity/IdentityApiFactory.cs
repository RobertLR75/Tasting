using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.IntegrationTests.Infrastructure;

namespace Tasting.Api.IntegrationTests.Identity;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
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

        context.Users.RemoveRange(context.Users);

        context.Users.Add(new User
        {
            Id = AdminId,
            Email = "admin@tasting.no",
            EmailNormalized = "admin@tasting.no",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task SeedUserAsync(User user)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
