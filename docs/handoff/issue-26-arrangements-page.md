# Spec: Arrangements Page (Issue #26)

**Issue:** https://github.com/RobertLR75/Tasting/issues/26
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

The action icon buttons in the admin list pages (`ArrangementsPage`, `UsersPage`, `BeersPage`, `BreweriesPage`) have no tooltip captions. Users cannot tell what an icon button does without clicking it. The issue requires hover captions (mouse-over tooltips) on every action icon button.

## Acceptance Criteria

1. All `MudIconButton` elements in the Actions column of `ArrangementsPage.razor`, `UsersPage.razor`, `BeersPage.razor`, and `BreweriesPage.razor` are wrapped with `MudTooltip` showing a descriptive English label.
2. Each `MudIconButton` also carries a matching `aria-label` attribute for accessibility.
3. The following tooltip / aria-label texts are used:

   | Page | Icon | Tooltip / aria-label |
   |------|------|----------------------|
   | ArrangementsPage | Edit | "Edit arrangement" |
   | ArrangementsPage | LocalDrink | "Manage beers" |
   | ArrangementsPage | People | "Manage participants" |
   | ArrangementsPage | PlayArrow | "Change status" |
   | UsersPage | Edit | "Edit user" |
   | UsersPage | Security | "Change role" |
   | UsersPage | ToggleOff | "Change status" |
   | BreweriesPage | Edit | "Edit brewery" |
   | BreweriesPage | LocalDrink | "Manage beers" |
   | BeersPage | Edit | "Edit beer" |

4. Static markup tests in `tests/Tasting.Admin.UnitTests/AdminFrontendPagesTests.cs` assert that `MudTooltip` is present in each affected page.
5. No existing behaviour or navigation is changed.

## Implementation Plan

1. Open `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor`.
   - Wrap each `MudIconButton` in the Actions `MudStack` with `<MudTooltip Text="...">...</MudTooltip>`.
   - Add `aria-label="..."` to each `MudIconButton` matching the tooltip text from the table above.

2. Open `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor`.
   - Apply the same wrapping pattern for its three icon buttons.

3. Open `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor`.
   - Apply the same wrapping pattern for its two icon buttons.

4. Open `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor`.
   - Apply the same wrapping pattern for its single Edit icon button.

5. Open `tests/Tasting.Admin.UnitTests/AdminFrontendPagesTests.cs`.
   - Add test methods (or inline assertions in existing methods) that read each affected `.razor` file and assert it contains `MudTooltip`.

6. Run `dotnet test tests/Tasting.Admin.UnitTests` to confirm all tests pass.

## Files to Change

| File | What to change |
|------|----------------|
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor` | Wrap 4 `MudIconButton`s in `MudTooltip`; add `aria-label` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor` | Wrap 3 `MudIconButton`s in `MudTooltip`; add `aria-label` |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor` | Wrap 2 `MudIconButton`s in `MudTooltip`; add `aria-label` |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor` | Wrap 1 `MudIconButton` in `MudTooltip`; add `aria-label` |
| `tests/Tasting.Admin.UnitTests/AdminFrontendPagesTests.cs` | Add static markup tests asserting `MudTooltip` presence |

## Tests

In `tests/Tasting.Admin.UnitTests/AdminFrontendPagesTests.cs`, add tests similar to the existing pattern:

```csharp
[Theory]
[InlineData("src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor")]
[InlineData("src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor")]
[InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor")]
[InlineData("src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor")]
public void ListPages_ActionButtons_ShouldHaveTooltips(string relativePath)
{
    var markup = File.ReadAllText(GetProjectFile(relativePath));
    Assert.Contains("MudTooltip", markup);
}
```

Run with: `dotnet test tests/Tasting.Admin.UnitTests`

## Out of Scope

- Adding tooltips to non-icon buttons (e.g. `ActionButton` text buttons like "Search", "Add Arrangement").
- Changing any navigation routes or business logic.
- Adding tooltips to pages not listed above.

## Domain Terms

No new domain terms. See `docs/glossary.md` for full glossary.

## Suggested Skills for Implementing Agent

- `tdd` — write the markup test before adding the tooltip markup to keep the loop tight
- `code-review` — review the diff for consistency across all four pages before merging

## Handoff Prompt

You are implementing issue #26 "Arrangements Page" from https://github.com/RobertLR75/Tasting/issues/26.

Read the full spec at `docs/handoff/issue-26-arrangements-page.md` before making any changes.

Work on a `codex/` branch (e.g. `codex/issue-26-action-tooltips`). Do not commit to main.

Steps:
1. Add `MudTooltip` wrappers and `aria-label` attributes to all `MudIconButton` elements in `ArrangementsPage.razor`, `UsersPage.razor`, `BreweriesPage.razor`, and `BeersPage.razor` using the exact tooltip texts in the spec.
2. Add the `ListPages_ActionButtons_ShouldHaveTooltips` theory test to `tests/Tasting.Admin.UnitTests/AdminFrontendPagesTests.cs`.
3. Run `dotnet test tests/Tasting.Admin.UnitTests` and confirm all tests pass.
4. Commit and open a PR that closes issue #26.
