# Spec: Login Page (Issue #25)

**Issue:** https://github.com/RobertLR75/Tasting/issues/25
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

The Sign In button on the login page only responds to mouse clicks. Pressing Enter while focus is in the Email field — or anywhere outside the Password field — does not trigger login. This happens because `OnKeyDown="@HandleKeyDownAsync"` is only wired to the Password `MudTextField`; the Email field and the form as a whole have no keyboard-submit handler.

## Acceptance Criteria

1. Pressing Enter while the Email field is focused submits the form.
2. Pressing Enter while the Password field is focused submits the form.
3. If the Email field is empty or not a valid email address, an inline validation error is displayed and the API is not called.
4. If the Password field is empty, an inline validation error is displayed and the API is not called.
5. Successful login navigates to `/` as before.
6. API error handling (401/403 and unexpected errors) is unchanged.
7. The loading spinner and `Disabled` state on the button behave as before.

## Implementation Plan

1. **Add `LoginFormModel` to `LoginModels.cs`** — a mutable class with `[Required]` and `[EmailAddress]` on `Email`, and `[Required]` on `Password`. This is the EditForm binding target.

2. **Update `LoginPage.razor`:**
   - Add `@using System.ComponentModel.DataAnnotations` (or rely on existing global usings).
   - Replace the two `private string` fields (`email`, `password`) with a single `private LoginFormModel _model = new()`.
   - Wrap the inner content of `MudPaper` in `<EditForm Model="_model" OnValidSubmit="HandleLoginAsync">`.
   - Add `<DataAnnotationsValidator />` inside the EditForm.
   - Update both `MudTextField` bindings:
     - `@bind-Value="_model.Email"` with `For="() => _model.Email"` to wire inline validation.
     - `@bind-Value="_model.Password"` with `For="() => _model.Password"` to wire inline validation.
   - Remove `OnKeyDown="@HandleKeyDownAsync"` from the Password `MudTextField`.
   - Change the `MudButton` from `OnClick="@HandleLoginAsync"` to `ButtonType="ButtonType.Submit"` (so it acts as a native form-submit button).
   - Remove the `HandleKeyDownAsync` method entirely.
   - In `HandleLoginAsync`, remove the manual null-check guard clause (DataAnnotationsValidator ensures OnValidSubmit is only called when the form is valid). Map `_model` to `LoginRequest(_model.Email, _model.Password)` when calling `AuthApiClient.LoginAsync`.

3. **Verify** the existing `Console.WriteLine` debug lines (lines 84, 86, 92) — remove them as part of this cleanup since they are unrelated debug artefacts in production code.

## Files to Change

| File | What to change |
|---|---|
| `src/Frontend/Tasting.Admin/Features/Auth/Models/LoginModels.cs` | Add `LoginFormModel` class with DataAnnotations |
| `src/Frontend/Tasting.Admin/Features/Auth/Pages/LoginPage.razor` | Wrap in EditForm, bind to model, remove OnKeyDown, add DataAnnotationsValidator, clean up Console.WriteLines |

## Out of Scope

- No changes to backend authentication logic.
- No changes to `AuthApiClient`, `TastingAuthStateProvider`, or `LoginLayout`.
- No new tests were requested (frontend Blazor component tests are not part of the current test strategy for this project).
- No UX redesign — layout, styling, and MudBlazor component choices are unchanged.

## Domain Terms

- **Active user** — only active users can authenticate; the 401/403 handling already covers inactive users. See `CONTEXT.md`.

## Suggested Skills for Implementing Agent

- `tdd` — if the team decides to add Blazor bUnit tests for the login component in future.
- `code-review` — run after implementation to verify the fix is complete and no regressions introduced.
