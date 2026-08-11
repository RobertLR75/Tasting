using System.Data.Common;
using Dapper;
using SharedLibrary.Dapper;
using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.Dapper;

public abstract class PostgresSqlDapperStorageBase<T>(DbConnection connection) : DapperBase<T>(connection), IPostgresSqlStorageService<T>
    where T : class, IEntity
{
    public virtual async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = DateTimeOffset.UtcNow;

        await ExecuteInTransactionAsync(async transaction => { await CreateAsync(entity, transaction, cancellationToken); }, cancellationToken);

        return entity.Id;
    }

    private async Task CreateAsync(T entity,  DbTransaction transaction, CancellationToken cancellationToken)
    {
        var properties = GetMappedProperties();
        var columns = string.Join(", ", properties.Select(p => QuoteIdentifier(MapPropertyToColumn(p.Name))));
        var values = string.Join(", ", properties.Select(p => "@" + p.Name));
        var sql = $"INSERT INTO {QuoteIdentifier(TableName)} ({columns}) VALUES ({values});";

        await Connection.ExecuteAsync(new CommandDefinition(
            sql,
            entity,
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await ExecuteInTransactionAsync(async transaction =>
        {
            await UpdateAsync(entity, transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task UpdateAsync(T entity, DbTransaction transaction, CancellationToken cancellationToken)
    {
        var properties = GetMappedProperties()
            .Where(p => p.Name != nameof(IEntity.Id))
            .ToArray();

        var setClause = string.Join(", ", properties.Select(p =>
            $"{QuoteIdentifier(MapPropertyToColumn(p.Name))} = @{p.Name}"));

        var sql =
            $"UPDATE {QuoteIdentifier(TableName)} SET {setClause} WHERE {QuoteIdentifier(MapPropertyToColumn(nameof(IEntity.Id)))} = @{nameof(IEntity.Id)};";

        await Connection.ExecuteAsync(new CommandDefinition(
            sql,
            entity,
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async transaction =>
        {
            await DeleteAsync(id, transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task DeleteAsync(Guid id, DbTransaction transaction, CancellationToken cancellationToken)
    {
        var sql =
            $"DELETE FROM {QuoteIdentifier(TableName)} WHERE {QuoteIdentifier(MapPropertyToColumn(nameof(IEntity.Id)))} = @{nameof(IEntity.Id)};";

        await Connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id },
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    public virtual async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        var sql =
            $"SELECT {GetSelectProjection()} FROM {QuoteIdentifier(TableName)} WHERE {QuoteIdentifier(MapPropertyToColumn(nameof(IEntity.Id)))} = @{nameof(IEntity.Id)};";

        return await Connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public virtual async Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        await EnsureConnectionOpenAsync(cancellationToken);

        var query = CreateSpecificationTranslator().Translate(specification);
        var entities = await Connection.QueryAsync<T>(new CommandDefinition(
            query.Sql,
            query.Parameters,
            cancellationToken: cancellationToken));

        return entities.AsList();
    }

    public virtual async Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var results = await SearchAsync(specification, cancellationToken);
        return results.FirstOrDefault()
               ?? throw new InvalidOperationException("Entity matching specification was not found.");
    }

    public virtual async Task<List<TResult>> SearchAsync<TResult>(
        IPersistenceSpecification<T, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        await EnsureConnectionOpenAsync(cancellationToken);

        var query = CreateSpecificationTranslator().Translate(specification);
        var results = await Connection.QueryAsync<TResult>(new CommandDefinition(
            query.Sql,
            query.Parameters,
            cancellationToken: cancellationToken));

        return results.AsList();
    }

    protected virtual IReadOnlyCollection<DapperRelationship> Relationships => [];

    private PostgreSqlSpecificationTranslator<T> CreateSpecificationTranslator()
        => new(
            TableName,
            MapPropertyToColumn,
            Relationships,
            GetSelectProjection("root"));


}
