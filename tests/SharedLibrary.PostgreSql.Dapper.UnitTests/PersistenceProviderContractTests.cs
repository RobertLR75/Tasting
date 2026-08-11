using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedLibrary.Interfaces;
using SharedLibrary.Dapper;
using SharedLibrary.PostgreSql.EntityFramework;
using Testcontainers.PostgreSql;

namespace SharedLibrary.PostgreSql.Dapper.UnitTests;

public class PersistenceProviderContractTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("persistence_contract")
        .WithUsername("tasting")
        .WithPassword("tasting")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "test_entity" (
                "id" uuid PRIMARY KEY,
                "created_at_utc" timestamp with time zone NOT NULL,
                "updated_at_utc" timestamp with time zone NULL,
                "name" text NOT NULL,
                "alcohol" integer NOT NULL,
                "is_active" boolean NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [Theory]
    [InlineData("EntityFramework")]
    [InlineData("Dapper")]
    public async Task Provider_ImplementsIdenticalIdSpecificationAndProjectionContract(string provider)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        var options = new DbContextOptionsBuilder<GenericDbContext<TestEntity>>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var context = new GenericDbContext<TestEntity>(options);
        IPersistenceService<TestEntity> service = provider == "EntityFramework"
            ? new EntityFrameworkStorage(context)
            : new DapperStorage(connection);
        var entity = new TestEntity { Name = $"{provider} IPA", Alcohol = 6, IsActive = true };

        var id = await service.CreateAsync(entity);
        var byId = await service.GetAsync(id);
        var matching = await service.SearchAsync(new ActiveEntitiesSpecification());
        var singleMatching = await service.GetAsync(new EntityByIdSpecification(id));
        var projected = await service.SearchAsync(new EntitySummarySpecification());

        Assert.Equal(entity.Name, byId?.Name);
        Assert.Contains(matching, candidate => candidate.Id == id);
        Assert.Equal(id, singleMatching.Id);
        Assert.Contains(projected, candidate => candidate.Id == id && candidate.Name == entity.Name);

        entity.Name = $"Updated {provider}";
        await service.UpdateAsync(entity);
        Assert.Equal(entity.Name, (await service.GetAsync(id))?.Name);

        await service.DeleteAsync(id);
        Assert.Null(await service.GetAsync(id));
    }

    [Fact]
    public async Task DapperBase_UsesSuppliedTransactionAndRollsBackOwnedTransactionOnFailure()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        var sut = new ExposedDapperBase(connection);
        await using (var supplied = await connection.BeginTransactionAsync())
        {
            sut.Transaction = supplied;
            var observed = false;
            await sut.ExecuteAsync(transaction =>
            {
                observed = ReferenceEquals(supplied, transaction);
                return Task.CompletedTask;
            });
            Assert.True(observed);
            await supplied.RollbackAsync();
        }

        sut.Transaction = null;
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(async transaction =>
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (NpgsqlTransaction)transaction;
            command.CommandText = "INSERT INTO test_entity (id, created_at_utc, name, alcohol, is_active) VALUES (@id, now(), 'rollback', 1, true)";
            command.Parameters.AddWithValue("id", Guid.NewGuid());
            await command.ExecuteNonQueryAsync();
            throw new InvalidOperationException("force rollback");
        }));

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT count(*) FROM test_entity WHERE name = 'rollback'";
        Assert.Equal(0L, await countCommand.ExecuteScalarAsync());
        Assert.Contains("\"Name\" AS \"Name\"", sut.Projection);
        Assert.Equal("Name", sut.Map("Name"));
    }

    private sealed class ActiveEntitiesSpecification : PersistenceSpecification<TestEntity>
    {
        public ActiveEntitiesSpecification()
        {
            Query.Where(entity => entity.IsActive).OrderBy(entity => entity.Name);
        }
    }

    private sealed class EntitySummarySpecification : PersistenceSpecification<TestEntity, EntitySummary>
    {
        public EntitySummarySpecification()
        {
            Query.Select(entity => new EntitySummary(entity.Id, entity.Name));
        }
    }

    private sealed class EntityByIdSpecification : PersistenceSpecification<TestEntity>
    {
        public EntityByIdSpecification(Guid id)
        {
            Query.Where(entity => entity.Id == id);
        }
    }

    private sealed class EntityFrameworkStorage(GenericDbContext<TestEntity> context)
        : EntityFrameworkPostgresSqlStorageBase<TestEntity>(context);

    private sealed class DapperStorage(NpgsqlConnection connection)
        : PostgresSqlDapperStorageBase<TestEntity>(connection)
    {
        protected override string TableName => "test_entity";

        protected override string MapPropertyToColumn(string propertyName) => propertyName switch
        {
            nameof(TestEntity.Id) => "id",
            nameof(TestEntity.CreatedAt) => "created_at_utc",
            nameof(TestEntity.UpdatedAt) => "updated_at_utc",
            nameof(TestEntity.Name) => "name",
            nameof(TestEntity.Alcohol) => "alcohol",
            nameof(TestEntity.IsActive) => "is_active",
            _ => propertyName
        };
    }

    private sealed record EntitySummary(Guid Id, string Name);

    private sealed class ExposedDapperBase(NpgsqlConnection connection) : DapperBase<TestEntity>(connection)
    {
        protected override string TableName => "test_entity";
        public string Projection => GetSelectProjection();
        public string Map(string propertyName) => MapPropertyToColumn(propertyName);
        public Task ExecuteAsync(Func<System.Data.Common.DbTransaction, Task> action)
            => ExecuteInTransactionAsync(action, CancellationToken.None);
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Alcohol { get; set; }
        public bool IsActive { get; set; }
    }
}
