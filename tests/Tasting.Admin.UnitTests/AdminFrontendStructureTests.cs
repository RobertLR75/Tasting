namespace Tasting.Admin.UnitTests;

public sealed class AdminFrontendStructureTests
{
    [Fact]
    public void NavMenu_ShouldContainAllExpectedAdminLinks()
    {
        var markup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Components/Shell/NavMenu.razor"));

        Assert.Contains("new(\"/\", \"Dashboard\"", markup);
        Assert.Contains("\"Dashboard\"", markup);
        Assert.Contains("\"Arrangements\"", markup);
        Assert.Contains("\"Users\"", markup);
        Assert.Contains("\"Beers\"", markup);
        Assert.Contains("\"Breweries\"", markup);
        Assert.Contains("\"Ratings\"", markup);
        Assert.Contains("\"Results\"", markup);
    }

    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor", "Arrangements")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor", "Users")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor", "Beers")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor", "Breweries")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Results/Pages/RatingsPage.razor", "Ratings")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Results/Pages/ResultsPage.razor", "Results")]
    public void FeaturePage_ShouldContainExpectedHeading(string relativePath, string expectedHeading)
    {
        var markup = File.ReadAllText(GetProjectFile(relativePath));

        Assert.Contains(expectedHeading, markup);
    }

    [Fact]
    public void FormField_ShouldPropagateValueChangesToParentBinding()
    {
        var markup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Shared/Components/FormField.razor"));

        Assert.Contains("T=\"string\"", markup);
        Assert.Contains("Value=\"@Value\"", markup);
        Assert.Contains("ValueChanged=\"@HandleValueChanged\"", markup);
        Assert.Contains("ValueChanged.InvokeAsync(value)", markup);
        Assert.DoesNotContain("@bind-value=\"Value\"", markup);
    }

    private static string GetProjectFile(string relativePath)
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
}
