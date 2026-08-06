# Spec: Arrangements Page (Issue #32)

**Issue:** https://github.com/RobertLR75/Tasting/issues/32
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

The Arrangements page renders all four action buttons (Edit, Add Beers, Add Participants, Status) regardless of the arrangement's lifecycle status. This allows users to attempt operations that the API will reject — e.g. editing a Started or Canceled arrangement.

## Acceptance Criteria

1. For a `Created` arrangement, all four action buttons are enabled (no change to current behaviour).
2. For a `Started` arrangement, the Edit, Add Beers, and Add Participants buttons are rendered but visually disabled (greyed out, non-clickable). The Status button remains enabled.
3. For a `Canceled` arrangement, the Edit, Add Beers, and Add Participants buttons are rendered but visually disabled. The Status button remains enabled.
4. For a `Completed` arrangement, all four buttons are rendered but visually disabled (no valid next transition).
5. Each disabled button is wrapped in a `MudTooltip` with a message explaining why it is disabled (e.g. `"Not available for Started arrangements"`).
6. Disabling logic lives in private helper methods in the `@code` block, not as inline ternaries in the markup.
7. No backend changes are required; the API already enforces status rules server-side.

## Implementation Plan

1. Open `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor`.
2. Add three private helper methods in `@code`:
   - `IsEditDisabled(ArrangementStatus status)` → returns `true` for `Started`, `Canceled`, `Completed`.
   - `IsAddContentDisabled(ArrangementStatus status)` → same logic (shared by Edit, Add Beers, Add Participants).
   - `IsStatusDisabled(ArrangementStatus status)` → returns `true` for `Completed` only.
3. In the `<RowTemplate>`, wrap each `MudIconButton` that can be disabled with a `MudTooltip`. Apply `Disabled="@IsAddContentDisabled(context.Status)"` (or `IsStatusDisabled`) to the relevant buttons.
4. Tooltip text:
   - Edit / Add Beers / Add Participants: `"Not available for @context.Status arrangements"` (or a static string per status group is fine).
   - Status: `"No further status transitions available"`.
5. Verify visually and confirm the page still works as expected for `Created` arrangements.

## Files to Change

| File | What to change |
|------|---------------|
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/ArrangementsPage.razor` | Add `Disabled` binding and `MudTooltip` wrappers to the four action buttons; add helper methods in `@code` |

## Tests

No existing automated tests cover the Blazor page directly. The behaviour is self-verifiable by running the frontend and checking each status. If a bUnit test project exists in the future, add a test that asserts button disabled states per status.

## Out of Scope

- Backend / API changes (API already enforces status rules).
- Any changes to the Edit, Add Beers, Add Participants, or Status sub-pages themselves.
- Adding new arrangement statuses or lifecycle transitions.

## Domain Terms

From `CONTEXT.md`:
- **Arrangement status**: `Created → Started → Completed`, or `Created → Canceled`. No other transitions are valid.
- **Arrangement**: A tasting session with a lifecycle; status governs which operations are permitted.

## Suggested Skills for Implementing Agent

- `code-review` — verify the disabled-state logic covers all four statuses correctly before committing.

## Handoff Prompt

```
Read the spec at docs/handoff/issue-32-arrangements-page.md and implement it.

Issue: https://github.com/RobertLR75/Tasting/issues/32

You are working on branch robertlr75-analyse-issue-32-arrangements-page (or create a new codex/ branch if starting fresh).

Steps:
1. Read the spec fully.
2. Edit ArrangementsPage.razor as described — add helper methods and MudTooltip + Disabled bindings.
3. Verify the change compiles (dotnet build src/Frontend/Tasting.Admin).
4. Open a PR referencing issue #32. Do NOT use "Closes #32" — the issue stays open until the PR is merged.
```
