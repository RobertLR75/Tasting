using Testcontainers.PostgreSql;

namespace Tasting.Api.IntegrationTests.Infrastructure;

internal sealed class PostgresContainerFixture : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("tasting")
        .WithUsername("tasting")
        .WithPassword("tasting")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
