using Tasting.Admin.UnitTests.Builders;

namespace Tasting.Admin.UnitTests;

public sealed class AdminFrontendPagesTests
{
    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/AddUserPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/EditUserPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeRolePage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeStatusPage.razor")]
    public void UserPages_ShouldExist(string relativePath)
    {
        var fullPath = GetProjectFile(relativePath);
        Assert.True(File.Exists(fullPath), $"File not found: {relativePath}");
    }

    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/AddBreweryPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/EditBreweryPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/AddBeerPage.razor")]
    public void BreweryPages_ShouldExist(string relativePath)
    {
        var fullPath = GetProjectFile(relativePath);
        Assert.True(File.Exists(fullPath), $"File not found: {relativePath}");
    }

    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddArrangementPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/EditArrangementPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddBeersPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddParticipantsPage.razor")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/StatusChangePage.razor")]
    public void ArrangementPages_ShouldExist(string relativePath)
    {
        var fullPath = GetProjectFile(relativePath);
        Assert.True(File.Exists(fullPath), $"File not found: {relativePath}");
    }

    [Fact]
    public void UserPages_ShouldHaveApiClientInjections()
    {
        var usersPageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor"));
        Assert.Contains("IUsersApiClient", usersPageMarkup);
    }

    [Fact]
    public void BreweryPages_ShouldHaveApiClientInjections()
    {
        var breweriesPageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor"));
        Assert.Contains("IBreweriesApiClient", breweriesPageMarkup);
    }

    [Fact]
    public void ArrangementPages_ShouldHaveApiClientInjections()
    {
        var arrangementsPageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor"));
        Assert.Contains("IArrangementsApiClient", arrangementsPageMarkup);
    }

    private static string GetProjectFile(string relativePath)
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
}
