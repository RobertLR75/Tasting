using Ardalis.Specification;
using SharedLibrary.HttpClient;
using SharedLibrary.Interfaces;

namespace SharedLibrary.HttpClient.UnitTests;

public class SpecificationQueryStringBuilderTests
{
    [Fact]
    public void BuildQueryString_ReturnsEmpty_WhenSpecificationIsNull()
    {
        var result = SpecificationQueryStringBuilder.BuildQueryString<TestRequest>(null);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void BuildQueryString_SerializesFiltersSearchSortAndPaging()
    {
        var specification = new BeerSpecification();

        var result = SpecificationQueryStringBuilder.BuildQueryString(specification);

        Assert.Contains("filter=", result);
        Assert.Contains("Name%20eq%20%27IPA%27", result);
        Assert.Contains("search=Name~%27ale%27%3Bgroup%3D2", result);
        Assert.Contains("sort=CreatedAt%3Adesc", result);
        Assert.Contains("skip=10", result);
        Assert.Contains("take=20", result);
    }

    [Fact]
    public void BuildQueryString_ThrowsForUnsupportedMethod()
    {
        var specification = new UnsupportedSpecification();

        var exception = Assert.Throws<NotSupportedException>(() => SpecificationQueryStringBuilder.BuildQueryString(specification));

        Assert.Contains("ToLower", exception.Message);
    }

    private sealed class BeerSpecification : ApiSpecification<TestRequest>
    {
        public BeerSpecification()
        {
            Query.Where(beer => beer.Name == "IPA" && beer.IsActive);
            Query.Search(beer => beer.Name, "ale", 2);
            Query.OrderByDescending(beer => beer.CreatedAt);
            Query.Skip(10);
            Query.Take(20);
        }
    }

    private sealed class UnsupportedSpecification : ApiSpecification<TestRequest>
    {
        public UnsupportedSpecification()
        {
            Query.Where(beer => beer.Name.ToLower() == "ipa");
        }
    }

    private sealed class TestRequest
    {
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
