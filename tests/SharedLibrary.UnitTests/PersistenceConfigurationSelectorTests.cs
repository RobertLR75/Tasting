using Microsoft.Extensions.Configuration;
using SharedLibrary.Configuration;

namespace SharedLibrary.UnitTests;

public class PersistenceConfigurationSelectorTests
{
    [Theory]
    [InlineData(null, PersistenceProvider.EntityFramework)]
    [InlineData("EntityFramework", PersistenceProvider.EntityFramework)]
    [InlineData("Dapper", PersistenceProvider.Dapper)]
    public void Select_ReturnsConfiguredProviderOrEntityFrameworkDefault(
        string? configuredProvider,
        PersistenceProvider expected)
    {
        var configuration = BuildConfiguration(configuredProvider, "Host=localhost;Database=tasting");

        var result = PersistenceConfigurationSelector.Select(configuration);

        Assert.Equal(expected, result.Provider);
    }

    [Fact]
    public void Select_RejectsUnknownProvider()
    {
        var configuration = BuildConfiguration("MongoDB", "Host=localhost;Database=tasting");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PersistenceConfigurationSelector.Select(configuration));

        Assert.Equal(
            "Unsupported persistence provider 'MongoDB'. Expected EntityFramework or Dapper.",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Select_RejectsMissingOrBlankConnectionString(string? connectionString)
    {
        var configuration = BuildConfiguration("EntityFramework", connectionString);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PersistenceConfigurationSelector.Select(configuration));

        Assert.Equal("ConnectionStrings:TastingDb must be configured and non-blank.", exception.Message);
    }

    private static IConfiguration BuildConfiguration(string? provider, string? connectionString)
    {
        var values = new Dictionary<string, string?>();
        if (provider is not null)
        {
            values["Persistence:Provider"] = provider;
        }

        if (connectionString is not null)
        {
            values["ConnectionStrings:TastingDb"] = connectionString;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
