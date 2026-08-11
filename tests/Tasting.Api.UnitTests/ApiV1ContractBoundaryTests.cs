using System.Reflection;

namespace Tasting.Api.UnitTests;

public sealed class ApiV1ContractBoundaryTests
{
    [Fact]
    public void PublicFeatureModels_DoNotExposeInternalVersions()
    {
        var forbiddenProperties = typeof(Program).Assembly
            .GetTypes()
            .Where(type => type.IsPublic && type.Namespace?.StartsWith("Tasting.Api.Features", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.Name.Contains("Version", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Contains("ETag", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Contains("ConcurrencyToken", StringComparison.OrdinalIgnoreCase))
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToArray();

        Assert.Empty(forbiddenProperties);
    }
}
