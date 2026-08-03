namespace SharedLibrary.Services.Interfaces;

public interface IEventReceiver 
{
    Task ReceiveEventAsync<T>(
        string message,
        Func<T?, Task> handler,
        Func<Task>? onCompleteMessage = null);
}