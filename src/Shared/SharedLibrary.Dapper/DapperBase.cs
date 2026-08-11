using System.Data;
using System.Data.Common;
using System.Reflection;
using SharedLibrary.Interfaces;

namespace SharedLibrary.Dapper;

public abstract class DapperBase<T>(DbConnection connection)
    where T : class, IEntity
{
    protected readonly DbConnection Connection = connection;

    public DbTransaction? Transaction { get; set; }

    protected abstract string TableName { get; }

    protected virtual string MapPropertyToColumn(string propertyName) => propertyName;

    protected async Task ExecuteInTransactionAsync(Func<DbTransaction, Task> action, CancellationToken cancellationToken)
    {
        if (Transaction is not null)
        {
            await action(Transaction);
            return;
        }

        await EnsureConnectionOpenAsync(cancellationToken);

        await using var transaction = await Connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await action(transaction);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    protected static PropertyInfo[] GetMappedProperties()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .ToArray();
    }

    protected string GetSelectProjection(string? tableAlias = null)
    {
        var prefix = tableAlias is null ? string.Empty : $"{QuoteIdentifier(tableAlias)}.";
        return string.Join(", ", GetMappedProperties().Select(p =>
            $"{prefix}{QuoteIdentifier(MapPropertyToColumn(p.Name))} AS {QuoteIdentifier(p.Name)}"));
    }

    protected static string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    protected async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync(cancellationToken);
        }
    }
}
