namespace SharedLibrary.Interfaces;

public interface IEntityId
{
    public Guid Id { get; set; }
}
public interface IEntity : IEntityId
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface INotification
{
    public string Id { get; set; }
}