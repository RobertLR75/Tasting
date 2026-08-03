using System.Reflection;
using SharedLibrary.Interfaces;

namespace SharedLibrary.Modeling;

public enum GenericEntityColumnKind
{
    String,
    Int32,
    DateTimeOffset,
    DateTime,
    Guid,
    Boolean,
    Int64,
    Decimal
}

public readonly record struct GenericEntityPropertyRule(
    string PropertyName,
    Type PropertyType,
    Type UnderlyingType,
    GenericEntityColumnKind ColumnKind,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsNavigationEntity,
    bool IsEnumString);

public static class GenericEntitySchema
{
    public static GenericEntityPropertyRule BuildPropertyRule(PropertyInfo property, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(entityType);

        var type = property.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        var isPrimaryKey = IsIdProperty(property);
        var isNavigationEntity = IsNavigationEntityType(underlyingType, entityType);
        var isEnumString = underlyingType.IsEnum;
        var columnKind = ResolveColumnKind(underlyingType, isEnumString);

        return new GenericEntityPropertyRule(
            property.Name,
            property.PropertyType,
            underlyingType,
            columnKind,
            isNullable,
            isPrimaryKey,
            isNavigationEntity,
            isEnumString);
    }

    public static bool ShouldCreateNavigationTable(Type propertyType, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(propertyType);
        ArgumentNullException.ThrowIfNull(entityType);

        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return IsNavigationEntityType(underlyingType, entityType);
    }

    public static PropertyInfo GetIdProperty(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        var idProperty = entityType.GetProperty(nameof(IEntity.Id), BindingFlags.Public | BindingFlags.Instance);
        if (idProperty is null || !IsSupportedKeyType(Nullable.GetUnderlyingType(idProperty.PropertyType) ?? idProperty.PropertyType))
        {
            throw new InvalidOperationException($"Type '{entityType.Name}' must declare a supported public Id property.");
        }

        return idProperty;
    }

    public static IReadOnlyList<PropertyInfo> GetProperties(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public static string GetTableName(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return entityType.Name + "s";
    }

    private static GenericEntityColumnKind ResolveColumnKind(Type underlyingType, bool isEnumString)
    {
        if (isEnumString || underlyingType == typeof(string))
            return GenericEntityColumnKind.String;
        if (underlyingType == typeof(int))
            return GenericEntityColumnKind.Int32;
        if (underlyingType == typeof(DateTimeOffset))
            return GenericEntityColumnKind.DateTimeOffset;
        if (underlyingType == typeof(DateTime))
            return GenericEntityColumnKind.DateTime;
        if (underlyingType == typeof(Guid))
            return GenericEntityColumnKind.Guid;
        if (underlyingType == typeof(bool))
            return GenericEntityColumnKind.Boolean;
        if (underlyingType == typeof(long))
            return GenericEntityColumnKind.Int64;
        if (underlyingType == typeof(decimal))
            return GenericEntityColumnKind.Decimal;

        return GenericEntityColumnKind.String;
    }

    private static bool IsIdProperty(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return property.Name.Equals(nameof(IEntity.Id), StringComparison.OrdinalIgnoreCase)
               && IsSupportedKeyType(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
    }

    private static bool IsNavigationEntityType(Type underlyingType, Type entityType)
    {
        return underlyingType != typeof(string)
               && underlyingType.IsClass
               && underlyingType != typeof(object)
               && underlyingType != entityType
               && underlyingType.GetProperty(nameof(IEntity.Id), BindingFlags.Public | BindingFlags.Instance) is not null;
    }

    private static bool IsSupportedKeyType(Type underlyingType)
    {
        return underlyingType == typeof(string)
               || underlyingType == typeof(Guid)
               || underlyingType == typeof(int)
               || underlyingType == typeof(long);
    }
}

