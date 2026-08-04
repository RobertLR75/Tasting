using SharedLibrary.Interfaces;

namespace SharedLibrary.UnitTests;

public class MessageDeserializerTests
{
    [Fact]
    public void Deserialize_ReturnsEvent_WhenTypeImplementsSharedEvent()
    {
        var sut = new MessageDeserializer<TestSharedEvent>();
        var id = Guid.NewGuid();
        var message = $$"""{"Id":"{{id}}","SchemaVersion":"1.0","OccurredAtUtc":"2026-08-04T00:00:00Z","CorrelationId":"corr-1"}""";

        var result = sut.Deserialize(message);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public void Deserialize_ReturnsDefault_WhenTypeDoesNotImplementSharedEvent()
    {
        var sut = new MessageDeserializer<NonSharedEvent>();

        var result = sut.Deserialize("""{"Name":"ignored"}""");

        Assert.Null(result);
    }

    private sealed class TestSharedEvent : ISharedEvent
    {
        public Guid Id { get; set; }
        public string SchemaVersion { get; init; } = "1.0";
        public DateTimeOffset OccurredAtUtc { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class NonSharedEvent
    {
        public string Name { get; init; } = string.Empty;
    }
}
