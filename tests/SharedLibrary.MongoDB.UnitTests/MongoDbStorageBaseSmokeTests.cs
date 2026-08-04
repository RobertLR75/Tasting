using System.Reflection;
using SharedLibrary.Interfaces;
using SharedLibrary.MongoDB;

namespace SharedLibrary.MongoDB.UnitTests;

public class MongoDbStorageBaseSmokeTests
{
    [Fact]
    public void CollectionName_IsDeclaredByDerivedType()
    {
        var property = typeof(MongoDbStorageBase<TestEntity>)
            .GetProperty("CollectionName", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(property);
        Assert.True(property!.GetMethod!.IsAbstract);
    }

    [Fact]
    public void SearchAsync_WithSearchFilter_HasExpectedShape()
    {
        var method = typeof(MongoDbStorageBase<TestEntity>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == "SearchAsync" && method.GetParameters()[0].ParameterType == typeof(SearchFilter<TestEntity>));

        Assert.Equal(typeof(Task<List<TestEntity>>), method.ReturnType);
    }

    [Fact]
    public void SearchAsync_WithSpecification_HasExpectedShape()
    {
        var method = typeof(MongoDbStorageBase<TestEntity>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == "SearchAsync" && method.GetParameters()[0].ParameterType == typeof(IPersistenceSpecification<TestEntity>));

        Assert.Equal(typeof(Task<List<TestEntity>>), method.ReturnType);
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
