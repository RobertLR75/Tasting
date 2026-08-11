using Microsoft.Extensions.Configuration;

namespace SharedLibrary.Configuration;

public enum PersistenceProvider
{
    EntityFramework,
    Dapper
}

public sealed record PersistenceConfiguration(PersistenceProvider Provider, string ConnectionString);

public static class PersistenceConfigurationSelector
{
    public static PersistenceConfiguration Select(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var configuredProvider = configuration["Persistence:Provider"]; 
        var provider = string.IsNullOrWhiteSpace(configuredProvider)
            ? PersistenceProvider.EntityFramework
            : configuredProvider.Trim() switch
            {
                var value when value.Equals(nameof(PersistenceProvider.EntityFramework), StringComparison.OrdinalIgnoreCase)
                    => PersistenceProvider.EntityFramework,
                var value when value.Equals(nameof(PersistenceProvider.Dapper), StringComparison.OrdinalIgnoreCase)
                    => PersistenceProvider.Dapper,
                _ => throw new InvalidOperationException(
                    $"Unsupported persistence provider '{configuredProvider}'. Expected EntityFramework or Dapper.")
            };

        var connectionString = configuration.GetConnectionString("TastingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:TastingDb must be configured and non-blank.");
        }

        return new PersistenceConfiguration(provider, connectionString);
    }
}
