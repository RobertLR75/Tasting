using Newtonsoft.Json;
using SharedLibrary.Interfaces;

namespace SharedLibrary;

public interface IMessageDeserializer<out T>
{
    T? Deserialize(string message);
}

public class MessageDeserializer<T> : IMessageDeserializer<T>
{
    public T? Deserialize(string message) =>
        typeof(ISharedEvent).IsAssignableFrom(typeof(T))
            ? JsonConvert.DeserializeObject<T>(message)
            : default;
}