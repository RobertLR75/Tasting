# Spec: Add Participants to Arrangement Page (Issue #28)

**Issue:** https://github.com/RobertLR75/Tasting/issues/28
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

The "Add Participants to Arrangement" page crashes on load with a JSON deserialization error because `UserDto.Id` is typed as `int` in the frontend model while the API returns a `Guid`. Additionally, the `AddParticipantAsync` method passes `Guid.Empty` instead of the actual user ID, and lacks the `RowVersion` required by the backend's optimistic-concurrency check. The page is non-functional end-to-end.

## Acceptance Criteria

1. Navigating to `/arrangements/{id}/participants` loads the page without errors.
2. The user list is populated correctly with name and email.
3. Users already added as participants are shown with the Add button disabled (greyed out).
4. Clicking Add on an eligible user successfully calls `POST /api/v1/arrangements/{id}/participants` with the correct `UserId` and `RowVersion`.
5. After a successful add, the user's row is disabled (not removed), reflecting they are now a participant.
6. `RowVersion` is fetched on page load and incremented locally after each successful add.
7. The three Identity sub-pages (`EditUserPage`, `ChangeRolePage`, `ChangeStatusPage`) route with `:guid` constraint and bind `Guid UserId`, preventing broken navigation from the Users page.
8. The `GET /api/v1/arrangements/{id}` response includes the arrangement's current participants list.

## Implementation Plan

1. **Backend — extend `ArrangementResponse`**
   - Add `record ArrangementParticipantResponse(Guid Id, Guid UserId, string UserName)` to `src/Backend/Tasting.Api/Features/Arrangement/ArrangementResponse.cs` (alongside the existing `ArrangementResponse` record or in the same file).
   - Add `IReadOnlyList<ArrangementParticipantResponse> Participants` property to `ArrangementResponse`.
   - Update `GetArrangementMapper.FromEntityAsync` (`src/Backend/Tasting.Api/Features/Arrangement/Arrangements/GetArrangement/GetArrangementMapper.cs`) to map `entity.Participants` → list of `ArrangementParticipantResponse` (using `FirstNameSnapshot + LastNameSnapshot` as `UserName`, or empty string if blank — snapshot fix is out of scope).

2. **Frontend — fix `UserDto`**
   - Change `UserDto.Id` from `int` to `Guid` in `src/Frontend/Tasting.Admin/Features/Identity/Models/UserDto.cs`.

3. **Frontend — fix `IUsersApiClient` / `UsersApiClient`**
   - Change `GetAsync(int id)`, `UpdateAsync(int id, ...)`, `ChangeRoleAsync(int id, ...)`, `ChangeStatusAsync(int id, ...)` signatures to `Guid id` in `src/Frontend/Tasting.Admin/Features/Identity/Services/UsersApiClient.cs`.

4. **Frontend — fix Identity sub-pages**
   - `src/Frontend/Tasting.Admin/Features/Identity/Pages/EditUserPage.razor`: `@page "/users/{UserId:int}/edit"` → `:guid`, `int UserId` → `Guid UserId`.
   - `src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeRolePage.razor`: same change.
   - `src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeStatusPage.razor`: same change.
   - `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor`: `EditUser(int id)` → `EditUser(Guid id)`.

5. **Frontend — extend `ArrangementDto`**
   - Add `IEnumerable<ArrangementParticipantDto> Participants` to `ArrangementDto` in `src/Frontend/Tasting.Admin/Features/Arrangement/Models/ArrangementModels.cs`.

6. **Frontend — fix `AddParticipantsPage.razor`**
   - On `OnInitializedAsync`: fetch `ArrangementDto` via `ArrangementsApiClient.GetAsync(ArrangementId)` to get `RowVersion` and existing `Participants`. Store in `private ArrangementDto? _arrangement`.
   - Change `AddingParticipantIds` to `HashSet<Guid>`.
   - Change `AddParticipantAsync(int userId)` → `AddParticipantAsync(Guid userId)`.
   - Build `AddParticipantToArrangementRequest(userId, _arrangement!.RowVersion)` (not `Guid.Empty`).
   - After a successful add, increment `_arrangement.RowVersion` locally (RowVersion++ equivalent — create a local `uint _rowVersion` field and sync from `ArrangementDto`).
   - In the user table, disable the Add button if `_arrangement.Participants.Any(p => p.UserId == context.Id)` or if `AddingParticipantIds.Contains(context.Id)`.
   - After a successful add, add the userId to a local `_addedUserIds` HashSet (or re-fetch participants) to keep the disabled state current without navigating away.

## Files to Change

| File | What to change |
|------|---------------|
| `src/Backend/Tasting.Api/Features/Arrangement/ArrangementResponse.cs` | Add `ArrangementParticipantResponse` record; add `Participants` list to `ArrangementResponse` |
| `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/GetArrangement/GetArrangementMapper.cs` | Map `entity.Participants` to `ArrangementParticipantResponse` list |
| `src/Frontend/Tasting.Admin/Features/Identity/Models/UserDto.cs` | `int Id` → `Guid Id` |
| `src/Frontend/Tasting.Admin/Features/Identity/Services/UsersApiClient.cs` | All `int id` params → `Guid id` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/EditUserPage.razor` | `:int` → `:guid`, `int UserId` → `Guid UserId` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeRolePage.razor` | `:int` → `:guid`, `int UserId` → `Guid UserId` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/ChangeStatusPage.razor` | `:int` → `:guid`, `int UserId` → `Guid UserId` |
| `src/Frontend/Tasting.Admin/Features/Identity/Pages/UsersPage.razor` | `EditUser(int id)` → `EditUser(Guid id)` |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Models/ArrangementModels.cs` | Add `Participants` to `ArrangementDto` |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddParticipantsPage.razor` | Full fix: Guid ids, RowVersion, existing-participant disable logic |

## Tests

- No existing test projects for frontend Blazor components were found. If a test project exists under `src/Frontend/`, add component tests for `AddParticipantsPage`.
- Backend: add or update integration tests in the existing backend test project (if present) for `GET /api/v1/arrangements/{id}` to assert the `participants` array is included in the response.

## Out of Scope

- Fixing `FirstNameSnapshot` / `LastNameSnapshot` population in `AddParticipantHandler` (handler currently stores `string.Empty`).
- Adding a dedicated `GET /arrangements/{id}/participants` endpoint.
- Any UI for removing participants from the page.

## Domain Terms

- **Participant**: A `User` who has been added to an `Arrangement`. Represented by `ArrangementParticipant` in the domain. See `CONTEXT.md` for full glossary.
- **RowVersion**: Optimistic concurrency token on `Arrangement`. Incremented by the server on every mutation; the client must supply the current value.

## Suggested Skills for Implementing Agent

- `tdd` — write failing tests before fixing the mapper and page code
- `code-review` — run after implementation to verify the fix against this spec

## Handoff Prompt

```
Read the spec at docs/handoff/issue-28-add-participants-to-arrangement-page.md.

Issue: https://github.com/RobertLR75/Tasting/issues/28

Work on a branch named `codex/fix-issue-28-participants-page` branched from `main`.

Follow the Implementation Plan in the spec exactly:
1. Extend ArrangementResponse (backend) to include participants.
2. Fix UserDto.Id int→Guid and all downstream UsersApiClient/identity-page usages.
3. Fix AddParticipantsPage.razor: correct Guid types, fetch arrangement on load for RowVersion, pass real userId, disable already-added users.

Run `dotnet build` across the solution to verify no compilation errors.
Open a PR titled "fix: participants page crash and broken add flow (#28)" when done.
```
