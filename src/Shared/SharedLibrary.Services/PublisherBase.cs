using Microsoft.Extensions.Logging;
using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.Services;

public abstract class PublisherBase<TEvent, TSharedEvent>(IEventPublisher eventPublisher, ILogger<PublisherBase<TEvent, TSharedEvent>> logger) : IEventPublisherService<TEvent>
    where TEvent : class
    where TSharedEvent : class, ISharedEvent

{
    protected readonly ILogger Logger = logger;
    protected readonly IEventPublisher EventPublisher = eventPublisher;

    protected abstract Task<TSharedEvent?> HandleEventAsync(TEvent ev);
   

    public async Task PublishAsync(TEvent eventModel, CancellationToken cancellation = default)
    {
        Logger.LogInformation("Event received:[{EventModel}]", eventModel);
        var sharedEvent = await HandleEventAsync(eventModel);

        if (sharedEvent != null)
            await EventPublisher.PublishAsync(eventModel, cancellation);
    }
}

public static class EventSchemaVersion
{
    public const string V1 = "1.0";
}