using FastEndpoints;

namespace SharedLibrary.Services.Interfaces;

public interface IEventPublisher
{
    /// <summary>
    /// Serialize the specified model and sends a message to the service bus.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ev">The model to publish</param>
    /// <param name="cancellationToken"></param>
    /// <param name="topicName">The topic to publish the message on. (Defaults to message.Subject)</param>
    /// <returns></returns>
    Task PublishAsync<T>(T ev, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Sends a message to the service bus.
    // /// </summary>
    // /// <param name="message">The message to publish</param>
    // /// <param name="topicName">The topic to publish the message on. (Defaults to message.Subject)</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    //Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default, string? topicName = null);
}