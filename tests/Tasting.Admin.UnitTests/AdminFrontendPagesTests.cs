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

    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddBeersPage.razor", "Failed to add beer: {ex.Message}")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddParticipantsPage.razor", "Failed to add participant: {ex.Message}")]
    public void MembershipPages_ShowSharedApiError_WithoutConcurrencyState(
        string relativePath,
        string expectedError)
    {
        var markup = File.ReadAllText(GetProjectFile(relativePath));

        Assert.Contains(expectedError, markup);
        Assert.DoesNotContain("RowVersion", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ArrangementConflictException", markup);
    }

    [Fact]
    public void StatusChangePage_ShouldUseMudSelect_NotMudTextField_ForStatusField()
    {
        var statusChangePageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/StatusChangePage.razor"));

        Assert.Contains("MudSelect T=\"ArrangementStatus\"", statusChangePageMarkup);
        Assert.Contains("MudSelectItem", statusChangePageMarkup);
        Assert.Contains("@bind-Value=\"@selectedNewStatus\"", statusChangePageMarkup);
        Assert.DoesNotContain("<FormField", statusChangePageMarkup);
        Assert.DoesNotContain("selectedStatus", statusChangePageMarkup);
        Assert.DoesNotContain("Enum.TryParse", statusChangePageMarkup);
    }

    [Theory]
    [InlineData("ArrangementStatus.Created => [ArrangementStatus.Active, ArrangementStatus.Canceled]")]
    [InlineData("ArrangementStatus.Active => [ArrangementStatus.Started]")]
    [InlineData("ArrangementStatus.Started => [ArrangementStatus.Completed]")]
    [InlineData("ArrangementStatus.Canceled => [ArrangementStatus.Created]")]
    [InlineData("ArrangementStatus.Completed => []")]
    public void StatusChangePage_ShouldPopulateOnlyValidTransitions(string expectedTransition)
    {
        var statusChangePageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/StatusChangePage.razor"));

        Assert.Contains("GetValidTransitions", statusChangePageMarkup);
        Assert.Contains(expectedTransition, statusChangePageMarkup);
    }

    [Fact]
    public void StatusChangePage_ShouldUseReopenActionForCanceledToCreated()
    {
        var statusChangePageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/StatusChangePage.razor"));

        Assert.Contains("ArrangementStatus.Created when currentArrangement!.Status == ArrangementStatus.Canceled => ArrangementsApiClient.ReopenAsync", statusChangePageMarkup);
        Assert.Contains("_ => UnsupportedStatus(newStatus)", statusChangePageMarkup);
    }

    [Fact]
    public void StatusChangePage_ShouldShowCompletedGuard()
    {
        var statusChangePageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/StatusChangePage.razor"));

        Assert.Contains("validTransitions.Count == 0", statusChangePageMarkup);
        Assert.Contains("Severity=\"Severity.Info\"", statusChangePageMarkup);
        Assert.Contains("No further status transitions are available", statusChangePageMarkup);
        Assert.Contains("There are no further transitions possible", statusChangePageMarkup);
    }

    [Fact]
    public void RootPage_ShouldShowArrangementsList()
    {
        var arrangementsPageMarkup = File.ReadAllText(GetProjectFile("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor"));
        var dashboardPagePath = GetProjectFile("src/Frontend/Tasting.Admin/Features/Dashboard/Pages/DashboardPage.razor");

        Assert.Contains("@page \"/\"", arrangementsPageMarkup);
        Assert.Contains("@page \"/arrangements\"", arrangementsPageMarkup);
        Assert.False(File.Exists(dashboardPagePath), "The old placeholder dashboard must not own the root route.");
        Assert.DoesNotContain("Admin Dashboard", arrangementsPageMarkup);
        Assert.DoesNotContain("Blank admin shell", arrangementsPageMarkup);
    }

    [Theory]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor", "Edit arrangement")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor", "Manage beers")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor", "Manage participants")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor", "Change status")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor", "Edit user")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor", "Change role")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor", "Change status")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor", "Edit brewery")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor", "Manage beers")]
    [InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor", "Edit beer")]
    public void ListPages_ActionButtons_ShouldHaveTooltipsAndAriaLabels(string relativePath, string expectedLabel)
    {
        var markup = File.ReadAllText(GetProjectFile(relativePath));
        Assert.Contains($"aria-label=\"{expectedLabel}\"", markup);

        if (relativePath.Contains("ArrangementsPage.razor", StringComparison.Ordinal))
        {
            Assert.Contains(expectedLabel, markup);
            return;
        }

        Assert.Contains($"MudTooltip Text=\"{expectedLabel}\"", markup);
        Assert.Contains($"aria-label=\"{expectedLabel}\"", markup);
    }

    private static string GetProjectFile(string relativePath)
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
}
