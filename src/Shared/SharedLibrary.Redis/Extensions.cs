using Microsoft.Extensions.Hosting;

namespace SharedLibrary.Redis;

public static class Extensions
{
    public static void ConfigureRedisDistributedCache(this IHostApplicationBuilder builder, string cacheName="cache")
    {
        builder.AddRedisDistributedCache("cache");
    }
    
    
}