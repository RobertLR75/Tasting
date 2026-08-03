using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.Services;

public sealed class EventReceiver(IServiceProvider serviceProvider) : IEventReceiver
{
    public async Task ReceiveEventAsync<T>(
        string message,
        Func<T?, Task> handler,
        Func<Task>? onCompleteMessage = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var deserializer = serviceProvider.GetRequiredService<IMessageDeserializer<T>>();
        var ev = deserializer.Deserialize(message);

        await handler(ev);

        if (onCompleteMessage != null)
        {
            await onCompleteMessage();
        }
    }
}
