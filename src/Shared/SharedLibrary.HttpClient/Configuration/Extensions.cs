using Duende.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Configuration;

namespace SharedLibrary.HttpClient.Configuration;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void ConfigureClientCredentialsTokenManagement(IdentityClientSettings settings)
        {
            GenericExtensions.ConfigureClientCredentialsTokenManagement(builder.Services, settings);
        }
    }
}

public static class HostBuilderExtensions
{
    extension(IHostBuilder builder)
    {
        public IHostBuilder ConfigureClientCredentialsTokenManagement(IdentityClientSettings settings)
        {
            return builder.ConfigureServices(services => { GenericExtensions.ConfigureClientCredentialsTokenManagement(services, settings); });
        }
    }
}

public static class GenericExtensions
{
    public static void ConfigureClientCredentialsTokenManagement(IServiceCollection services, IdentityClientSettings settings)
    {
        var scopes = string.Join(" ", settings.Scopes).Replace(",", "");

        services.AddClientCredentialsTokenManagement()
            .AddClient(settings.Name, client =>
            {
                client.TokenEndpoint = settings.TokenEndpoint;
                client.ClientId = ClientId.Parse(settings.ClientId);
                client.ClientSecret = ClientSecret.Parse(settings.ClientSecret);
                client.Scope = Scope.Parse(scopes);
            });
    }
    
}