using Dapper;

namespace SharedLibrary.PostgreSql.Dapper;

public sealed record SqlSpecificationQuery(
    string Sql,
    DynamicParameters Parameters,
    IReadOnlyList<DapperRelationship> Relationships);

public sealed class DapperRelationship
{
    private readonly Action<object, object?> _attach;

    private DapperRelationship(
        string navigationProperty,
        string tableName,
        string sourceColumn,
        string targetColumn,
        Type relatedType,
        Func<string, string> columnName,
        Action<object, object?> attach)
    {
        NavigationProperty = navigationProperty;
        TableName = tableName;
        SourceColumn = sourceColumn;
        TargetColumn = targetColumn;
        RelatedType = relatedType;
        ColumnName = columnName;
        _attach = attach;
    }

    public string NavigationProperty { get; }
    public string TableName { get; }
    public string SourceColumn { get; }
    public string TargetColumn { get; }
    public Type RelatedType { get; }
    internal Func<string, string> ColumnName { get; }

    internal void Attach(object root, object? related) => _attach(root, related);

    public static DapperRelationship Reference<TRoot, TRelated>(
        string navigationProperty,
        string tableName,
        string sourceColumn,
        string targetColumn,
        Action<TRoot, TRelated?> attach,
        Func<string, string>? columnName = null)
        where TRoot : class
        where TRelated : class
        => new(
            navigationProperty,
            tableName,
            sourceColumn,
            targetColumn,
            typeof(TRelated),
            columnName ?? (name => name),
            (root, related) => attach((TRoot)root, (TRelated?)related));

    public static DapperRelationship Collection<TRoot, TRelated>(
        string navigationProperty,
        string tableName,
        string sourceColumn,
        string targetColumn,
        Func<TRoot, ICollection<TRelated>> collection,
        Func<string, string>? columnName = null)
        where TRoot : class
        where TRelated : class
        => new(
            navigationProperty,
            tableName,
            sourceColumn,
            targetColumn,
            typeof(TRelated),
            columnName ?? (name => name),
            (root, related) =>
            {
                if (related is not TRelated item)
                {
                    return;
                }

                var target = collection((TRoot)root);
                if (item is SharedLibrary.Interfaces.IEntityId entity &&
                    target.OfType<SharedLibrary.Interfaces.IEntityId>().Any(existing => existing.Id == entity.Id))
                {
                    return;
                }

                target.Add(item);
            });
}
