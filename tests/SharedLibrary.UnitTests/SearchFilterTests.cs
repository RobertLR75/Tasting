using System.Linq.Expressions;
using SharedLibrary.Interfaces;

namespace SharedLibrary.UnitTests;

public class SearchFilterTests
{
    [Fact]
    public void SearchFilterCriterion_AllowsPropertySelector()
    {
        var criterion = new SearchFilterCriterion<TestEntity>(entity => entity.Name, "ipa");

        Assert.Equal("ipa", criterion.Value);
    }

    [Fact]
    public void SearchFilterCriterion_RejectsNonPropertySelector()
    {
        Expression<Func<TestEntity, object?>> selector = entity => entity.Name.ToUpperInvariant();

        var exception = Assert.Throws<ArgumentException>(() => new SearchFilterCriterion<TestEntity>(selector, "ipa"));

        Assert.Contains("Field selector must reference a property", exception.Message);
    }

    [Fact]
    public void SearchSortCriterion_RejectsNonPropertySelector()
    {
        Expression<Func<TestEntity, object?>> selector = entity => entity.Name + "!";

        var exception = Assert.Throws<ArgumentException>(() => new SearchSortCriterion<TestEntity>(selector));

        Assert.Contains("Field selector must reference a property", exception.Message);
    }

    private sealed class TestEntity
    {
        public string Name { get; init; } = string.Empty;
    }
}
