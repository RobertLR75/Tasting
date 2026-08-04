using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using SharedLibrary.Interfaces;
using SharedLibrary.Redis;

namespace SharedLibrary.Redis.UnitTests;

public class BaseRedisPersistenceServiceTests
{
    [Fact]
    public async Task CreateAsync_AssignsIdStoresEntityAndUpdatesIndex()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("beer:index", Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var sut = new TestRedisService(cache);
        var entity = new TestEntity();

        var id = await sut.CreateAsync(entity, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        await cache.Received().SetAsync(Arg.Is<string>(key => key == $"beer:{id}"), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
        await cache.Received().SetAsync("beer:index", Arg.Is<byte[]>(value => System.Text.Encoding.UTF8.GetString(value).Contains(id.ToString(), StringComparison.Ordinal)), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Lager", CreatedAt = DateTimeOffset.UtcNow };
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"beer:{entity.Id}", Arg.Any<CancellationToken>()).Returns(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entity)));
        var sut = new TestRedisService(cache);

        var result = await sut.GetAsync(entity.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Lager", result.Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersAndSortsEntitiesFromCache()
    {
        var first = new TestEntity { Id = Guid.NewGuid(), Name = "IPA", Rating = 3, CreatedAt = DateTimeOffset.UtcNow };
        var second = new TestEntity { Id = Guid.NewGuid(), Name = "Stout", Rating = 5, CreatedAt = DateTimeOffset.UtcNow };
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("beer:index", Arg.Any<CancellationToken>()).Returns(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] { first.Id.ToString(), second.Id.ToString() })));
        cache.GetAsync($"beer:{first.Id}", Arg.Any<CancellationToken>()).Returns(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(first)));
        cache.GetAsync($"beer:{second.Id}", Arg.Any<CancellationToken>()).Returns(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(second)));
        var sut = new TestRedisService(cache);

        var result = await sut.SearchAsync(new SearchFilter<TestEntity>
        {
            Parameters = [new SearchFilterCriterion<TestEntity>(entity => entity.Name, "Stout")],
            SortFields = [new SearchSortCriterion<TestEntity>(entity => entity.Rating, SearchSortDirection.Descending)]
        });

        var only = Assert.Single(result);
        Assert.Equal("Stout", only.Name);
    }

    private sealed class TestRedisService(IDistributedCache cache) : BaseRedisPersistenceService<TestEntity>(cache)
    {
        public override string Name => "beer";
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
