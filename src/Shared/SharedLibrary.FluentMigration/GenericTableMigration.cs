using System.Linq.Expressions;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using SharedLibrary.Interfaces;
using SharedLibrary.Modeling;

namespace SharedLibrary.FluentMigration;

public class GenericTableMigration<TEntity> : Migration where TEntity : class, IDatabaseRecord
{
    internal readonly record struct ColumnMapping(
        Type UnderlyingType,
        bool IsNullable,
        bool IsPrimaryKey,
        bool IsNavigationEntity,
        bool IsEnumString);

    internal static ColumnMapping BuildColumnMapping(PropertyInfo property, Type entityType)
    {
        var rule = GenericEntitySchema.BuildPropertyRule(property, entityType);
        return new ColumnMapping(rule.UnderlyingType, rule.IsNullable, rule.IsPrimaryKey, rule.IsNavigationEntity, rule.IsEnumString);
    }

    internal static bool ShouldCreateNavigationTable(Type propertyType, Type entityType)
    {
        return GenericEntitySchema.ShouldCreateNavigationTable(propertyType, entityType);
    }

    public override void Up()
    {
        CreateTableForEntity(typeof(TEntity), new HashSet<Type>());
    }

    private void CreateTableForEntity(Type entityType, ISet<Type> visitedTypes)
    {
        if (!visitedTypes.Add(entityType))
        {
            return;
        }

        var tableName = GenericEntitySchema.GetTableName(entityType);
        var properties = GenericEntitySchema.GetProperties(entityType);

        foreach (var prop in properties)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (ShouldCreateNavigationTable(prop.PropertyType, entityType))
            {
                CreateTableForEntity(propType, visitedTypes);
            }
        }

        var table = Create.Table(tableName);
        bool hasId = false;

        foreach (var prop in properties)
        {
            var column = table.WithColumn(prop.Name);
            var mapping = BuildColumnMapping(prop, entityType);
            var isNullable = mapping.IsNullable;

            if (mapping.IsPrimaryKey)
            {
                ConfigureColumn(column, mapping.UnderlyingType, false, isPrimaryKey: true, propertyName: prop.Name);
                hasId = true;
            }
            else if (mapping.IsNavigationEntity)
            {
                var principalIdProperty = GenericEntitySchema.GetIdProperty(mapping.UnderlyingType);
                var principalIdType = Nullable.GetUnderlyingType(principalIdProperty.PropertyType) ?? principalIdProperty.PropertyType;
                ConfigureColumn(column, principalIdType, isNullable, propertyName: principalIdProperty.Name);
            }
            else
            {
                ConfigureColumn(column, mapping.UnderlyingType, isNullable, propertyName: prop.Name);
            }
        }
        // Create index for Id column if present
        if (hasId)
        {
            Create.Index($"IX_{tableName}_Id").OnTable(tableName).OnColumn("Id").Ascending().WithOptions().NonClustered();
        }

        // Create foreign keys for navigation properties
        foreach (var prop in properties)
        {
            var mapping = BuildColumnMapping(prop, entityType);
            if (mapping.IsNavigationEntity)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var referencedTable = GenericEntitySchema.GetTableName(propType);
                var referencedIdProperty = GenericEntitySchema.GetIdProperty(propType);
                var fkName = $"FK_{tableName}_{prop.Name}_{propType.Name}";
                Create.ForeignKey(fkName)
                    .FromTable(tableName).ForeignColumn(prop.Name)
                    .ToTable(referencedTable).PrimaryColumn(referencedIdProperty.Name);
            }
        }
    }

    public override void Down()
    {
        DeleteTableForEntity(typeof(TEntity), new HashSet<Type>());
    }

    private void DeleteTableForEntity(Type entityType, ISet<Type> visitedTypes)
    {
        if (!visitedTypes.Add(entityType))
        {
            return;
        }

        Delete.Table(GenericEntitySchema.GetTableName(entityType));

        var properties = GenericEntitySchema.GetProperties(entityType);
        foreach (var prop in properties)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (ShouldCreateNavigationTable(prop.PropertyType, entityType))
            {
                DeleteTableForEntity(propType, visitedTypes);
            }
        }
    }

    protected void CreateIndexForColumn(Expression<Func<TEntity, object?>> selector, bool unique = false)
    {
        var propertyName = GetPropertyName(selector);
        CreateIndexForColumn(propertyName, unique);
    }

    protected void CreateIndexForColumn(string propertyName, bool unique = false)
    {
        var entityType = typeof(TEntity);
        var tableName = GenericEntitySchema.GetTableName(entityType);
        var property = entityType.GetProperty(propertyName);
        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{entityType.Name}'.");

        var indexBuilder = Create.Index($"IX_{tableName}_{propertyName}")
            .OnTable(tableName)
            .OnColumn(propertyName).Ascending();

        if (unique)
        {
            indexBuilder.WithOptions().Unique();
        }

        indexBuilder.WithOptions().NonClustered();
    }

    private static string GetPropertyName(Expression<Func<TEntity, object?>> expr)
    {
        if (expr.Body is MemberExpression member)
            return member.Member.Name;

        if (expr.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            return unaryMember.Member.Name;

        throw new ArgumentException("Expression must be a simple member access", nameof(expr));
    }

    private static void ConfigureColumn(ICreateTableColumnAsTypeSyntax column, Type underlyingType, bool isNullable, bool isPrimaryKey = false, string? propertyName = null)
    {
        var columnSyntax = ResolveColumnType(column, underlyingType, propertyName);

        if (isPrimaryKey)
        {
            columnSyntax.PrimaryKey();
            return;
        }

        if (isNullable)
        {
            columnSyntax.Nullable();
        }
        else
        {
            columnSyntax.NotNullable();
        }
    }

    private static ICreateTableColumnOptionOrWithColumnSyntax ResolveColumnType(ICreateTableColumnAsTypeSyntax column, Type underlyingType, string? propertyName)
    {
        if (underlyingType.IsEnum || underlyingType == typeof(string))
        {
            return column.AsString(string.Equals(propertyName, "Id", StringComparison.OrdinalIgnoreCase) ? 36 : 200);
        }

        return underlyingType switch
        {
            _ when underlyingType == typeof(int) => column.AsInt32(),
            _ when underlyingType == typeof(DateTimeOffset) => column.AsDateTimeOffset(),
            _ when underlyingType == typeof(DateTime) => column.AsDateTime(),
            _ when underlyingType == typeof(Guid) => column.AsGuid(),
            _ when underlyingType == typeof(bool) => column.AsBoolean(),
            _ when underlyingType == typeof(long) => column.AsInt64(),
            _ when underlyingType == typeof(decimal) => column.AsDecimal(),
            _ => column.AsString(200)
        };
    }
}
