using Ardalis.Specification;
using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.Dapper.UnitTests;

public class PostgreSqlSpecificationTranslatorTests
{
    [Fact]
    public void Translate_ParameterizesComposedCriteria()
    {
        var minimumAlcohol = 5;
        var specification = new EntitySpecification();
        specification.Query.Where(entity => entity.IsActive &&
            (entity.Alcohol >= minimumAlcohol || entity.Name.Contains("IPA")));

        var result = CreateTranslator().Translate(specification);

        Assert.Equal(
            "SELECT root.* FROM \"beers\" AS root WHERE ((root.\"is_active\" = TRUE) AND ((root.\"alcohol\" >= @p0) OR (root.\"name\" LIKE @p1 ESCAPE '\\')));",
            result.Sql);
        Assert.Equal(5, result.Parameters.Get<int>("p0"));
        Assert.Equal("%IPA%", result.Parameters.Get<string>("p1"));
    }

    [Fact]
    public void Translate_UsesDistinctParametersAcrossSeparateCriteria()
    {
        var specification = new EntitySpecification();
        specification.Query
            .Where(entity => entity.Alcohol >= 5)
            .Where(entity => entity.Name == "IPA");

        var result = CreateTranslator().Translate(specification);

        Assert.Contains("root.\"alcohol\" >= @p0", result.Sql);
        Assert.Contains("root.\"name\" = @p1", result.Sql);
        Assert.Equal(5, result.Parameters.Get<int>("p0"));
        Assert.Equal("IPA", result.Parameters.Get<string>("p1"));
    }

    [Fact]
    public void Translate_AddsProjectionSortingAndPaging()
    {
        var specification = new ProjectionSpecification();
        specification.Query.Select(entity => new EntitySummary(entity.Id, entity.Name));
        specification.Query
            .OrderBy(entity => entity.Name)
            .ThenByDescending(entity => entity.Alcohol)
            .Skip(20)
            .Take(10);

        var result = CreateTranslator().Translate(specification);

        Assert.Equal(
            "SELECT root.\"id\" AS \"Id\", root.\"name\" AS \"Name\" FROM \"beers\" AS root ORDER BY root.\"name\" ASC, root.\"alcohol\" DESC LIMIT @__take OFFSET @__skip;",
            result.Sql);
        Assert.Equal(10, result.Parameters.Get<int>("__take"));
        Assert.Equal(20, result.Parameters.Get<int>("__skip"));
    }

    [Fact]
    public void Translate_AddsMappedRelationshipJoin()
    {
        var specification = new EntitySpecification();
        specification.Query.Include(entity => entity.Brewery!);
        var translator = new PostgreSqlSpecificationTranslator<TestEntity>(
            "beers",
            ToSnakeCase,
            [new DapperRelationship(nameof(TestEntity.Brewery), "breweries", "brewery_id", "id")]);

        var result = translator.Translate(specification);

        Assert.Equal(
            "SELECT root.* FROM \"beers\" AS root LEFT JOIN \"breweries\" AS \"rel_Brewery\" ON root.\"brewery_id\" = \"rel_Brewery\".\"id\";",
            result.Sql);
    }

    [Fact]
    public void Translate_RejectsUnsupportedConstructDeterministically()
    {
        var specification = new EntitySpecification();
        specification.Query.Where(entity => entity.Name.ToLower() == "ipa");

        var exception = Assert.Throws<NotSupportedException>(() =>
            CreateTranslator().Translate(specification));

        Assert.StartsWith(
            "The persistence specification contains an unsupported construct.",
            exception.Message);
    }

    [Fact]
    public void Translate_SupportsNegationNullAndReversedComparisons()
    {
        var specification = new EntitySpecification();
        specification.Query.Where(entity => !entity.IsActive ||
            (entity.Name != null && 10 > entity.Alcohol));

        var result = CreateTranslator().Translate(specification);

        Assert.Contains("NOT (root.\"is_active\" = TRUE)", result.Sql);
        Assert.Contains("root.\"name\" IS NOT NULL", result.Sql);
        Assert.Contains("root.\"alcohol\" < @p0", result.Sql);
    }

    [Theory]
    [InlineData("starts", "starts%")]
    [InlineData("ends", "%ends")]
    public void Translate_SupportsStringPatterns(string value, string expectedPattern)
    {
        var specification = new EntitySpecification();
        specification.Query.Where(value == "starts"
            ? entity => entity.Name.StartsWith(value)
            : entity => entity.Name.EndsWith(value));

        var result = CreateTranslator().Translate(specification);

        Assert.Equal(expectedPattern, result.Parameters.Get<string>("p0"));
    }

    [Fact]
    public void Translate_EscapesPostgreSqlLikeWildcards()
    {
        var specification = new EntitySpecification();
        specification.Query.Where(entity => entity.Name.Contains(@"100%_beer\style"));

        var result = CreateTranslator().Translate(specification);

        Assert.Contains("LIKE @p0 ESCAPE '\\'", result.Sql);
        Assert.Equal(@"%100\%\_beer\\style%", result.Parameters.Get<string>("p0"));
    }

    [Fact]
    public void Translate_SupportsScalarAndMemberInitializerProjections()
    {
        var scalar = new ScalarProjectionSpecification();
        scalar.Query.Select(entity => entity.Name);
        var initialized = new InitializedProjectionSpecification();
        initialized.Query.Select(entity => new MutableSummary { Id = entity.Id, Name = entity.Name });

        var scalarResult = CreateTranslator().Translate(scalar);
        var initializedResult = CreateTranslator().Translate(initialized);

        Assert.Contains("root.\"name\" AS \"Name\"", scalarResult.Sql);
        Assert.Contains("root.\"id\" AS \"Id\", root.\"name\" AS \"Name\"", initializedResult.Sql);
    }

    [Fact]
    public void Translate_RejectsUnmappedRelationship()
    {
        var specification = new EntitySpecification();
        specification.Query.Include(entity => entity.Brewery!);

        var exception = Assert.Throws<NotSupportedException>(() => CreateTranslator().Translate(specification));

        Assert.Contains("Relationship 'Brewery' has no Dapper mapping", exception.Message);
    }

    [Fact]
    public void Translate_RejectsPostProcessingSearchAndStringIncludes()
    {
        var postProcessing = new EntitySpecification();
        postProcessing.Query.PostProcessingAction(entities => entities);
        var search = new EntitySpecification();
        search.Query.Search(entity => entity.Name, "IPA");
        var stringInclude = new EntitySpecification();
        stringInclude.Query.Include(nameof(TestEntity.Brewery));

        Assert.Throws<NotSupportedException>(() => CreateTranslator().Translate(postProcessing));
        Assert.Throws<NotSupportedException>(() => CreateTranslator().Translate(search));
        Assert.Throws<NotSupportedException>(() => CreateTranslator().Translate(stringInclude));
    }

    [Fact]
    public void Translate_RejectsUnsafeIdentifiersAndIndirectProjection()
    {
        var plain = new EntitySpecification();
        var indirect = new ScalarProjectionSpecification();
        indirect.Query.Select(entity => entity.Name.Length.ToString());

        Assert.Throws<InvalidOperationException>(() =>
            new PostgreSqlSpecificationTranslator<TestEntity>("beers;drop table beers").Translate(plain));
        Assert.Throws<NotSupportedException>(() => CreateTranslator().Translate(indirect));
    }

    private static PostgreSqlSpecificationTranslator<TestEntity> CreateTranslator()
        => new("beers", ToSnakeCase);

    private static string ToSnakeCase(string property) => property switch
    {
        nameof(TestEntity.Id) => "id",
        nameof(TestEntity.Name) => "name",
        nameof(TestEntity.Alcohol) => "alcohol",
        nameof(TestEntity.IsActive) => "is_active",
        _ => property
    };

    private sealed class EntitySpecification : PersistenceSpecification<TestEntity>;
    private sealed class ProjectionSpecification : PersistenceSpecification<TestEntity, EntitySummary>;
    private sealed class ScalarProjectionSpecification : PersistenceSpecification<TestEntity, string>;
    private sealed class InitializedProjectionSpecification : PersistenceSpecification<TestEntity, MutableSummary>;
    private sealed record EntitySummary(Guid Id, string Name);
    private sealed class MutableSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Alcohol { get; set; }
        public bool IsActive { get; set; }
        public Guid BreweryId { get; set; }
        public Brewery? Brewery { get; set; }
    }

    private sealed class Brewery
    {
        public Guid Id { get; set; }
    }
}
