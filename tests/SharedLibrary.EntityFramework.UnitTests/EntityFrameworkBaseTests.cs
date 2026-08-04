using Microsoft.EntityFrameworkCore;
using SharedLibrary.EntityFramework;
using SharedLibrary.Interfaces;

namespace SharedLibrary.EntityFramework.UnitTests;

public class EntityFrameworkBaseTests
{
    [Fact]
    public async Task ExecuteInTransactionAsync_ExecutesActionWithoutTransaction_ForInMemoryProvider()
    {
        await using var context = CreateContext();
        var sut = new TestEntityFrameworkBase(context);
        var entity = new TestEntity { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, Name = "Stout" };

        await sut.ExecuteAsync(async () =>
        {
            await context.Set<TestEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
        });

        Assert.Single(context.Set<TestEntity>());
    }

    [Fact]
    public void BuildFilterPredicate_FiltersUsingLogicalOr()
    {
        var filter = new SearchFilter<TestEntity>
        {
            LogicalOperator = SearchLogicalOperator.Or,
            Parameters =
            [
                new SearchFilterCriterion<TestEntity>(entity => entity.Name, "IPA"),
                new SearchFilterCriterion<TestEntity>(entity => entity.Alcohol, 8)
            ]
        };

        var predicate = TestEntityFrameworkBase.BuildPredicate(filter).Compile();

        Assert.True(predicate(new TestEntity { Name = "IPA", Alcohol = 4 }));
        Assert.True(predicate(new TestEntity { Name = "Porter", Alcohol = 8 }));
        Assert.False(predicate(new TestEntity { Name = "Porter", Alcohol = 4 }));
    }

    [Fact]
    public void ApplySorting_UsesAllSortFieldsInOrder()
    {
        var data = new[]
        {
            new TestEntity { Name = "B", Alcohol = 5 },
            new TestEntity { Name = "A", Alcohol = 7 },
            new TestEntity { Name = "A", Alcohol = 4 }
        }.AsQueryable();

        var sorted = TestEntityFrameworkBase.ApplySort(
            data,
            [
                new SearchSortCriterion<TestEntity>(entity => entity.Name),
                new SearchSortCriterion<TestEntity>(entity => entity.Alcohol, SearchSortDirection.Descending)
            ]).ToList();

        Assert.Equal([7, 4, 5], sorted.Select(entity => entity.Alcohol));
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private sealed class TestEntityFrameworkBase(TestDbContext context) : EntityFrameworkBase<TestEntity>(context)
    {
        public Task ExecuteAsync(Func<Task> action) => ExecuteInTransactionAsync(action, CancellationToken.None);

        public static System.Linq.Expressions.Expression<Func<TestEntity, bool>> BuildPredicate(SearchFilter<TestEntity> filter)
            => BuildFilterPredicate(filter);

        public static IQueryable<TestEntity> ApplySort(IQueryable<TestEntity> query, List<SearchSortCriterion<TestEntity>> sortFields)
            => ApplySorting(query, sortFields);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Alcohol { get; set; }
    }
}
