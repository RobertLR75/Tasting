using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.Services.Configuration;

public static class Extensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(typeof(IMessageDeserializer<>), typeof(MessageDeserializer<>));
        services.AddSingleton<IEventReceiver, EventReceiver>();

        return services;
    }

    public static void ConfigureServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSharedServices();
    }
}