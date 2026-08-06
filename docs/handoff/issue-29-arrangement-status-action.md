# Spec: Arrangement Status Action (Issue #29)

**Issue:** https://github.com/RobertLR75/Tasting/issues/29  
**Date:** 2026-08-06  
**Status:** Ready for development

## Problem

Clicking the ▶ (status action) icon in the Arrangements list throws `System.ObjectDisposedException` inside Blazor's `RenderTreeDiffBuilder`. `ArrangementsPage.razor` loads data in `OnAfterRenderAsync` using a redundant auth guard, then calls `StateHasChanged()` after an async HTTP call. When the user navigates away to `/arrangements/{id}/status`, the component is disposed while the in-flight request is still running; it completes and attempts to re-render the disposed component, causing the crash.

## Acceptance Criteria

1. Clicking the ▶ status action icon on the Arrangements list navigates to `/arrangements/{id}/status` without throwing `ObjectDisposedException` or any unhandled circuit error.
2. `ArrangementsPage.razor` loads its data in `OnInitializedAsync`, consistent with all other pages in the codebase.
3. The redundant `AuthStateProvider.GetAuthenticationStateAsync()` call, the `_hasLoaded` guard field, and the explicit `StateHasChanged()` call are removed from `ArrangementsPage.razor`.
4. `ArrangementsPage`, `BeersPage`, `BreweriesPage`, and `UsersPage` each implement `IDisposable` and use a `CancellationTokenSource` to cancel their in-flight HTTP load if the component is disposed before the request completes.
5. No regression in the loading behaviour of any of the four affected pages when navigating normally.

## Implementation Plan

1. **`ArrangementsPage.razor`** — primary fix:
   - Remove `@inject TastingAuthStateProvider AuthStateProvider` (no longer needed).
   - Remove `@using Tasting.Admin.Features.Auth.Services` if it is only needed for the auth provider.
   - Replace `OnAfterRenderAsync` with `OnInitializedAsync` that calls `SearchAsync()` directly.
   - Remove the `_hasLoaded` field and the `StateHasChanged()` call.
   - Add `@implements IDisposable`.
   - Add a `CancellationTokenSource _cts = new()` field and pass `_cts.Token` to any awaited HTTP calls (or catch `OperationCanceledException` silently).
   - Implement `Dispose()`: `_cts.Cancel(); _cts.Dispose();`.

2. **`BeersPage.razor`** — preventive hardening (same pattern):
   - File: `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor`
   - Add `@implements IDisposable`, `CancellationTokenSource _cts = new()`.
   - Pass `_cts.Token` to `OnInitializedAsync` async work; silently swallow `OperationCanceledException`.
   - Implement `Dispose()`.

3. **`BreweriesPage.razor`** — same as step 2:
   - File: `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor`

4. **`UsersPage.razor`** — same as step 2:
   - File: `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor`

5. Verify that the `ArrangementsApiClient`, `BeersApiClient`, and `UsersApiClient` async methods accept a `CancellationToken` parameter — or catch `TaskCanceledException`/`OperationCanceledException` at the component level if the clients do not forward it.

## Files to Change

| File | What to change |
|------|---------------|
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor` | Migrate from `OnAfterRenderAsync`+auth guard to `OnInitializedAsync`; add `IDisposable`+`CancellationTokenSource` |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor` | Add `IDisposable`+`CancellationTokenSource` |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BreweriesPage.razor` | Add `IDisposable`+`CancellationTokenSource` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor` | Add `IDisposable`+`CancellationTokenSource` |

## Tests

No dedicated test projects exist for the Blazor frontend. Manual verification steps:

1. Navigate to `/arrangements`.
2. Before the list fully loads, immediately click the ▶ status icon — no unhandled circuit error should appear in the server log.
3. Complete the status change and confirm navigation back to `/arrangements` loads correctly.
4. Repeat steps 1–3 for the Beers and Users list pages by navigating away quickly during load.

If a bUnit test project exists or is created in future, add component tests for `ArrangementsPage` verifying `OnInitializedAsync` triggers data load and that disposing the component while the task is pending does not throw.

## Out of Scope

- Backend changes.
- Auth/authorization changes.
- Fixing any other pages beyond the four listed above.
- Adding a cancellation token to `IArrangementsApiClient` methods (done at component catch level to minimise diff).

## Domain Terms

- **Arrangement status** — lifecycle value (`Created → Started → Completed`, or `Created → Canceled`). See `CONTEXT.md`.

## Suggested Skills for Implementing Agent

- `tdd` — if the team adds bUnit tests for the fix
- `code-review` — to verify the cancellation pattern is consistent across all four pages after implementation

## Handoff Prompt

```
Read the spec at docs/handoff/issue-29-arrangement-status-action.md.

Fix issue #29 (https://github.com/RobertLR75/Tasting/issues/29): ObjectDisposedException when clicking the status action in the Arrangements list.

Work on a dedicated `codex/` branch (do NOT commit to main).

Steps:
1. Rewrite ArrangementsPage.razor to load in OnInitializedAsync (remove OnAfterRenderAsync, the redundant AuthStateProvider auth check, _hasLoaded, and StateHasChanged()). Add IDisposable + CancellationTokenSource.
2. Add IDisposable + CancellationTokenSource to BeersPage.razor, BreweriesPage.razor, and UsersPage.razor.
3. Run the solution build to confirm no compile errors.
4. Open a PR linking issue #29 and referencing this spec.
```
