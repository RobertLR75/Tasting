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

    [Theory]
    [InlineData(nameof(AllTypesEntity.Number), GenericEntityColumnKind.Int32)]
    [InlineData(nameof(AllTypesEntity.Offset), GenericEntityColumnKind.DateTimeOffset)]
    [InlineData(nameof(AllTypesEntity.Timestamp), GenericEntityColumnKind.DateTime)]
    [InlineData(nameof(AllTypesEntity.Id), GenericEntityColumnKind.Guid)]
    [InlineData(nameof(AllTypesEntity.Enabled), GenericEntityColumnKind.Boolean)]
    [InlineData(nameof(AllTypesEntity.LargeNumber), GenericEntityColumnKind.Int64)]
    [InlineData(nameof(AllTypesEntity.Amount), GenericEntityColumnKind.Decimal)]
    public void BuildPropertyRule_MapsSupportedScalarKinds(string propertyName, GenericEntityColumnKind expected)
    {
        var property = typeof(AllTypesEntity).GetProperty(propertyName)!;

        var rule = GenericEntitySchema.BuildPropertyRule(property, typeof(AllTypesEntity));

        Assert.Equal(expected, rule.ColumnKind);
    }

    [Fact]
    public void EntityMetadata_ReturnsIdPropertiesAndTableName()
    {
        Assert.Equal(nameof(AllTypesEntity.Id), GenericEntitySchema.GetIdProperty(typeof(AllTypesEntity)).Name);
        Assert.Contains(GenericEntitySchema.GetProperties(typeof(AllTypesEntity)), property => property.Name == nameof(AllTypesEntity.Amount));
        Assert.Equal("AllTypesEntitys", GenericEntitySchema.GetTableName(typeof(AllTypesEntity)));
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

    private sealed class AllTypesEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int Number { get; set; }
        public DateTimeOffset Offset { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Enabled { get; set; }
        public long LargeNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
