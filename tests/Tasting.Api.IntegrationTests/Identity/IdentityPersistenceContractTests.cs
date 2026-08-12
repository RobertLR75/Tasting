using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;
using Tasting.Api.Infrastructure.Migrations;
using Tasting.Api.IntegrationTests.Infrastructure;

namespace Tasting.Api.IntegrationTests.Identity;

[Collection("Identity provider matrix")]
public sealed class IdentityPersistenceContractTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgres = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        new TastingMigrationService().MigrateUp(_postgres.ConnectionString, []);
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [Fact]
    public async Task Provider_switch_preserves_user_role_and_active_state_without_conversion()
    {
        await ResetUsersAsync();
        var user = User("switch@tasting.no", UserRole.Admin, isActive: true);

        await WithProviderAsync(PersistenceProvider.EntityFramework, users => users.CreateAsync(user));

        var dapperRead = await WithProviderAsync(
            PersistenceProvider.Dapper,
            users => users.GetAsync(user.Id));
        Assert.NotNull(dapperRead);
        Assert.Equal(UserRole.Admin, dapperRead.Role);
        Assert.True(dapperRead.IsActive);

        dapperRead.Role = UserRole.User;
        dapperRead.IsActive = false;
        await WithProviderAsync(PersistenceProvider.Dapper, users => users.UpdateAsync(dapperRead));

        var entityFrameworkRead = await WithProviderAsync(
            PersistenceProvider.EntityFramework,
            users => users.GetAsync(user.Id));
        Assert.NotNull(entityFrameworkRead);
        Assert.Equal(UserRole.User, entityFrameworkRead.Role);
        Assert.False(entityFrameworkRead.IsActive);
    }

    [Theory]
    [InlineData(PersistenceProvider.EntityFramework)]
    [InlineData(PersistenceProvider.Dapper)]
    public async Task Provider_enforces_normalized_email_uniqueness(PersistenceProvider provider)
    {
        await ResetUsersAsync();
        await WithProviderAsync(provider, users => users.CreateAsync(User("unique@tasting.no")));

        await Assert.ThrowsAnyAsync<Exception>(() =>
            WithProviderAsync(provider, users => users.CreateAsync(User("UNIQUE@tasting.no"))));

        var matches = await WithProviderAsync(
            provider,
            users => users.SearchAsync(new UserByNormalizedEmailSpecification("unique@tasting.no")));
        Assert.Single(matches);
    }

    private async Task ResetUsersAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.ExecuteAsync("TRUNCATE TABLE users;");
    }

    private async Task<TResult> WithProviderAsync<TResult>(
        PersistenceProvider provider,
        Func<IPersistenceService<User>, Task<TResult>> action)
    {
        var services = new ServiceCollection();
        services.AddIdentityInfrastructure(new PersistenceConfiguration(provider, _postgres.ConnectionString));
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<IPersistenceService<User>>());
    }

    private async Task WithProviderAsync(
        PersistenceProvider provider,
        Func<IPersistenceService<User>, Task> action)
        => await WithProviderAsync(provider, async users =>
        {
            await action(users);
            return true;
        });

    private static User User(
        string email,
        UserRole role = UserRole.User,
        bool isActive = true)
        => new()
        {
            Email = email,
            EmailNormalized = email.ToLowerInvariant(),
            FirstName = "Contract",
            LastName = "User",
            PasswordHash = "hash",
            Role = role,
            IsActive = isActive
        };
}
