namespace SharedLibrary.Interfaces;

public interface IDatabaseRecord 
{
    public string Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}