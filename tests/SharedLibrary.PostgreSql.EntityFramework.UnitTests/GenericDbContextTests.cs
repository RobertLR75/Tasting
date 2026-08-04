using Microsoft.EntityFrameworkCore;
using SharedLibrary.Interfaces;
using SharedLibrary.PostgreSql.EntityFramework;

namespace SharedLibrary.PostgreSql.EntityFramework.UnitTests;

public class GenericDbContextTests
{
    [Fact]
    public void Model_MapsColumnsUsingConventions()
    {
        using var context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entity);
        Assert.Equal("test_entity", entity!.GetTableName());
        Assert.Equal("created_at_utc", entity.FindProperty(nameof(IEntity.CreatedAt))!.GetColumnName());
    }

    [Fact]
    public void Model_StoresEnumsAsStrings()
    {
        using var context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(TestEntity))!;
        var property = entity.FindProperty(nameof(TestEntity.Status))!;

        Assert.NotNull(property.GetValueConverter());
        Assert.Equal(typeof(string), property.GetValueConverter()!.ProviderClrType);
    }

    [Fact]
    public void Model_WithNullableForeignKeyType_MapsNavigationUsingConventions()
    {
        using var context = CreateNullableNavigationContext();

        var entity = context.Model.FindEntityType(typeof(NullableNavigationEntity));

        Assert.NotNull(entity);
        Assert.Equal("parent_id", entity!.FindProperty("ParentId")!.GetColumnName());
        Assert.NotNull(entity.FindNavigation(nameof(NullableNavigationEntity.Parent)));
    }

    private static GenericDbContext<TestEntity> CreateContext()
    {
        var options = new DbContextOptionsBuilder<GenericDbContext<TestEntity>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GenericDbContext<TestEntity>(options);
    }

    private static GenericDbContext<NullableNavigationEntity> CreateNullableNavigationContext()
    {
        var options = new DbContextOptionsBuilder<GenericDbContext<NullableNavigationEntity>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GenericDbContext<NullableNavigationEntity>(options);
    }

    private enum EntityStatus
    {
        Draft,
        Published
    }

    private sealed class ParentEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }

    private sealed class NullableNavigationEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public ParentEntity? Parent { get; set; }
    }
}
