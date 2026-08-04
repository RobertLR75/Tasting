using SharedLibrary.Interfaces;
using SharedLibrary.Modeling;

namespace SharedLibrary.UnitTests;

public class GenericEntitySchemaTests
{
    [Fact]
    public void BuildPropertyRule_RecognizesNavigationAndPrimaryKeyMetadata()
    {
        var property = typeof(SampleEntity).GetProperty(nameof(SampleEntity.Related))!;

        var rule = GenericEntitySchema.BuildPropertyRule(property, typeof(SampleEntity));

        Assert.Equal(nameof(SampleEntity.Related), rule.PropertyName);
        Assert.True(rule.IsNullable);
        Assert.False(rule.IsPrimaryKey);
        Assert.True(rule.IsNavigationEntity);
        Assert.Equal(GenericEntityColumnKind.String, rule.ColumnKind);
    }

    [Fact]
    public void BuildPropertyRule_RecognizesEnumAsString()
    {
        var property = typeof(SampleEntity).GetProperty(nameof(SampleEntity.Status))!;

        var rule = GenericEntitySchema.BuildPropertyRule(property, typeof(SampleEntity));

        Assert.True(rule.IsEnumString);
        Assert.Equal(GenericEntityColumnKind.String, rule.ColumnKind);
    }

    [Fact]
    public void GetIdProperty_ThrowsForUnsupportedIdType()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => GenericEntitySchema.GetIdProperty(typeof(UnsupportedEntity)));

        Assert.Contains("must declare a supported public Id property", exception.Message);
    }

    [Fact]
    public void ShouldCreateNavigationTable_ReturnsFalse_ForSelfReference()
    {
        var result = GenericEntitySchema.ShouldCreateNavigationTable(typeof(SampleEntity), typeof(SampleEntity));

        Assert.False(result);
    }

    private enum SampleStatus
    {
        Draft,
        Active
    }

    private sealed class SampleEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public RelatedEntity? Related { get; set; }
        public SampleStatus Status { get; set; }
    }

    private sealed class RelatedEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    private sealed class UnsupportedEntity
    {
        public decimal Id { get; set; }
    }
}
