using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.EntityFramework;

internal static class GenericDbContextJson
{
    internal static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}

internal static class GenericDbContextReflection
{
    internal static readonly NullabilityInfoContext NullabilityContext = new();
}

public class GenericDbContext<TEntity>(DbContextOptions<GenericDbContext<TEntity>> options)
    : DbContext(options)
    where TEntity : class, IEntity
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        BuildEntity(typeof(TEntity), new HashSet<Type>());
        base.OnModelCreating(modelBuilder);

        void BuildEntity(Type entityType, ISet<Type> visitedTypes)
        {
            if (!visitedTypes.Add(entityType))
            {
                return;
            }

            var properties = GetMappedProperties(entityType);

            foreach (var property in properties.Where(IsNavigationProperty))
            {
                BuildEntity(GetUnderlyingType(property.PropertyType), visitedTypes);
            }

            var entityBuilder = modelBuilder.Entity(entityType);
            var tableName = ToSnakeCase(entityType.Name);
            entityBuilder.ToTable(tableName);

            var keyProperty = ConfigurePrimaryKey(entityBuilder, entityType);

            foreach (var property in properties)
            {
                if (property.Name == keyProperty.Name)
                {
                    continue;
                }

                if (IsNavigationProperty(property))
                {
                    ConfigureNavigation(entityBuilder, property);
                    continue;
                }

                ConfigureScalarProperty(entityBuilder, property, tableName);
            }
        }
    }

    private static PropertyInfo ConfigurePrimaryKey(EntityTypeBuilder entityBuilder, Type entityType)
    {
        var idProperty = GetIdProperty(entityType);
        entityBuilder.HasKey(idProperty.Name);

        var propertyBuilder = entityBuilder.Property(idProperty.PropertyType, idProperty.Name)
            .HasColumnName(ToColumnName(idProperty.Name))
            .ValueGeneratedNever();

        ApplyStringLengthConvention(propertyBuilder, idProperty);
        return idProperty;
    }

    private static void ConfigureScalarProperty(EntityTypeBuilder entityBuilder, PropertyInfo property, string tableName)
    {
        var underlyingType = GetUnderlyingType(property.PropertyType);
        var isNullable = IsNullable(property);
        var columnName = ToColumnName(property.Name);

        if (underlyingType.IsEnum)
        {
            entityBuilder.Property(property.PropertyType, property.Name)
                .HasConversion(CreateEnumToStringConverter(underlyingType, isNullable))
                .HasColumnName(columnName)
                .HasMaxLength(20)
                .IsRequired(!isNullable);

            entityBuilder.HasIndex(property.Name)
                .HasDatabaseName($"ix_{tableName}_{columnName}");
            return;
        }

        var propertyBuilder = entityBuilder.Property(property.PropertyType, property.Name)
            .HasColumnName(columnName)
            .IsRequired(!isNullable);

        if (underlyingType == typeof(string))
        {
            ApplyStringLengthConvention(propertyBuilder, property);
            return;
        }

        if (IsSimpleScalarType(underlyingType))
        {
            return;
        }

        propertyBuilder.HasConversion(CreateJsonValueConverter(property.PropertyType))
            .HasMaxLength(200);
    }

    private static void ConfigureNavigation(EntityTypeBuilder entityBuilder, PropertyInfo property)
    {
        var navigationType = GetUnderlyingType(property.PropertyType);
        var principalKeyProperty = GetIdProperty(navigationType);
        var foreignKeyPropertyName = ToForeignKeyPropertyName(property.Name);
        var foreignKeyColumnName = ToForeignKeyColumnName(property.Name);
        var isNullable = IsNullable(property);
        var foreignKeyClrType = isNullable
            ? typeof(Nullable<>).MakeGenericType(principalKeyProperty.PropertyType)
            : principalKeyProperty.PropertyType;

        var shadowProperty = entityBuilder.Property(foreignKeyClrType, foreignKeyPropertyName)
            .HasColumnName(foreignKeyColumnName)
            .IsRequired(!isNullable);

        ApplyForeignKeyLengthConvention(shadowProperty, principalKeyProperty);

        entityBuilder.HasOne(navigationType, property.Name)
            .WithMany()
            .HasForeignKey(foreignKeyPropertyName)
            .HasPrincipalKey(principalKeyProperty.Name)
            .OnDelete(DeleteBehavior.Restrict);

        entityBuilder.Navigation(property.Name).AutoInclude();
    }

    private static void ApplyStringLengthConvention(PropertyBuilder propertyBuilder, PropertyInfo property)
    {
        var maxLength = property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
            ? 36
            : property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)
                ? 50
                : 200;

        propertyBuilder.HasMaxLength(maxLength);
    }

    private static void ApplyForeignKeyLengthConvention(PropertyBuilder propertyBuilder, PropertyInfo principalKeyProperty)
    {
        if (GetUnderlyingType(principalKeyProperty.PropertyType) == typeof(string))
        {
            propertyBuilder.HasMaxLength(36);
        }
    }

    private static IReadOnlyList<PropertyInfo> GetMappedProperties(Type entityType)
    {
        return entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetMethod is not null)
            .ToArray();
    }

    private static PropertyInfo GetIdProperty(Type entityType)
    {
        var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty is null)
        {
            throw new InvalidOperationException($"Type '{entityType.Name}' must declare a public Id property to be mapped by {nameof(GenericDbContext<TEntity>)}.");
        }

        return idProperty;
    }

    private static bool IsNavigationProperty(PropertyInfo property)
    {
        var underlyingType = GetUnderlyingType(property.PropertyType);
        return underlyingType != typeof(string)
               && underlyingType.IsClass
               && underlyingType != typeof(object)
               && underlyingType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance) is not null;
    }

    private static bool IsSimpleScalarType(Type type)
    {
        return type == typeof(string)
               || type == typeof(Guid)
               || type == typeof(DateTimeOffset)
               || type == typeof(DateTime)
               || type == typeof(bool)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(decimal)
               || type == typeof(double)
               || type == typeof(float)
               || type == typeof(short)
               || type == typeof(byte);
    }

    private static bool IsNullable(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (Nullable.GetUnderlyingType(property.PropertyType) is not null)
        {
            return true;
        }

        if (!property.PropertyType.IsValueType)
        {
            return GenericDbContextReflection.NullabilityContext.Create(property).WriteState == NullabilityState.Nullable;
        }

        return false;
    }

    private static Type GetUnderlyingType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    private static string ToForeignKeyPropertyName(string navigationPropertyName) => $"{navigationPropertyName}Id";

    private static string ToForeignKeyColumnName(string navigationPropertyName) => $"{ToSnakeCase(navigationPropertyName)}_id";

    private static string ToColumnName(string propertyName)
    {
        return propertyName switch
        {
            nameof(IEntity.CreatedAt) => "created_at_utc",
            nameof(IEntity.UpdatedAt) => "updated_at_utc",
            _ => ToSnakeCase(propertyName)
        };
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            var transformed = char.ToLowerInvariant(character);

            if (char.IsUpper(character))
            {
                var hasPrevious = index > 0;
                var nextIsLower = index + 1 < value.Length && char.IsLower(value[index + 1]);
                var previousIsLowerOrDigit = hasPrevious && (char.IsLower(value[index - 1]) || char.IsDigit(value[index - 1]));

                if (hasPrevious && (previousIsLowerOrDigit || nextIsLower))
                {
                    builder.Append('_');
                }
            }

            builder.Append(transformed);
        }

        return builder.ToString();
    }

    private static ValueConverter CreateEnumToStringConverter(Type enumType, bool isNullable)
    {
        var factory = typeof(GenericDbContext<TEntity>)
            .GetMethod(
                isNullable ? nameof(CreateNullableEnumToStringConverter) : nameof(CreateEnumToStringConverterCore),
                BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(enumType);

        return (ValueConverter)factory.Invoke(null, null)!;
    }

    private static ValueConverter<TEnum, string> CreateEnumToStringConverterCore<TEnum>() where TEnum : struct, Enum
    {
        return new ValueConverter<TEnum, string>(
            value => value.ToString(),
            value => (TEnum)Enum.Parse(typeof(TEnum), value));
    }

    private static ValueConverter<TEnum?, string?> CreateNullableEnumToStringConverter<TEnum>() where TEnum : struct, Enum
    {
        return new ValueConverter<TEnum?, string?>(
            value => value.HasValue ? value.Value.ToString() : null,
            value => string.IsNullOrWhiteSpace(value) ? null : (TEnum)Enum.Parse(typeof(TEnum), value));
    }

    private static ValueConverter CreateJsonValueConverter(Type propertyType)
    {
        var factory = typeof(GenericDbContext<TEntity>)
            .GetMethod(nameof(CreateJsonValueConverterCore), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(propertyType);

        return (ValueConverter)factory.Invoke(null, null)!;
    }

    private static ValueConverter<TProperty, string?> CreateJsonValueConverterCore<TProperty>()
    {
        return new ValueConverter<TProperty, string?>(
            value => JsonSerializer.Serialize(value, GenericDbContextJson.SerializerOptions),
            value => string.IsNullOrWhiteSpace(value)
                ? default!
                : JsonSerializer.Deserialize<TProperty>(value, GenericDbContextJson.SerializerOptions)!);
    }
}
