namespace SharedLibrary.Services.Interfaces;

public interface IEventPublisherService<in TEvent>
{
    public Task PublishAsync(TEvent ev, CancellationToken cancellation = default);
}