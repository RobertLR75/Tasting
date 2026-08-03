namespace SharedLibrary.Interfaces;

public interface ISharedEvent
{
    public Guid Id { get; set; }
    public string SchemaVersion { get; init; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string CorrelationId { get; set; }
}