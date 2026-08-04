using Bunit;
using Tasting.Admin.Shared.Components;
using Xunit;

namespace Tasting.Admin.UnitTests.Components;

public class SearchBarTests : TestContext
{
    [Fact]
    public void Renders_WithLabel()
    {
        var cut = RenderComponent<SearchBar>(parameters => parameters
            .Add(p => p.Label, "Search users...")
        );

        cut.MarkupMatches(
            @"<div class=""mud-paper mud-paper-filled mud-paper-filled-default pa-4 mb-4"">" +
            @"<div class=""mud-grid""><div class=""mud-grid-item xs12 sm8"">" +
            @"<label>Search users...</label></div>" +
            @"</div></div>"
        );
    }

    [Fact]
    public async Task Emits_SearchTerm_WhenButtonClicked()
    {
        var emitted = "";
        var cut = RenderComponent<SearchBar>(parameters => parameters
            .Add(p => p.Label, "Search")
            .Add(p => p.SearchTerm, "test")
            .Add(p => p.OnSearch, new EventCallback<string>(null,
                new Action<string>(x => emitted = x)))
        );

        var button = cut.Find("button");
        await button.ClickAsync(new());

        Assert.Equal("test", emitted);
    }
}

public class StatusBadgeTests : TestContext
{
    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Created")]
    public void Renders_WithCorrectStatus(string status)
    {
        var cut = RenderComponent<StatusBadge>(parameters => parameters
            .Add(p => p.Status, status)
        );

        var markup = cut.Markup;
        Assert.Contains(status, markup);
    }
}
