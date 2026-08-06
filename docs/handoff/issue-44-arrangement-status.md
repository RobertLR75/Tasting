# Spec: Arrangement Status (Issue #44)

**Issue:** https://github.com/RobertLR75/Tasting/issues/44
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

The Arrangement lifecycle is missing an intermediate `Active` status between `Created` and `Started`. Without it, admins cannot signal that an arrangement has been confirmed and locked down for preparation before opening it for rating.

## Acceptance Criteria

1. `ArrangementStatus` enum in both Domain and Contracts contains exactly five values: `Created`, `Active`, `Started`, `Canceled`, `Completed`.
2. A new `POST /arrangements/{id}/activate` endpoint transitions an arrangement from `Created → Active`. Returns the updated `ArrangementResponse`. Requires `Admin` role and a matching `RowVersion`.
3. `StartArrangement` (`POST /arrangements/{id}/start`) only accepts arrangements in `Active` status; a `Created` arrangement must be activated first.
4. `CancelArrangement` only accepts `Created` status (unchanged). `Active` arrangements cannot be cancelled.
5. There is no `Active → Created` rollback. `Active` is a one-way commitment.
6. All operations previously gated on `Created` (AddBeer, RemoveBeer, AddParticipant, RemoveParticipant, UpdateArrangement) remain gated on `Created` only; they are rejected when the arrangement is `Active`.
7. The Admin frontend `ArrangementsApiClient` exposes an `ActivateAsync(Guid id, uint rowVersion)` method calling `POST /arrangements/{id}/activate`.
8. The existing `ChangeStatusAsync` mismatch (calling non-existent `/change-status`) is corrected to use the appropriate dedicated endpoints.
9. ADR 0003 and `CONTEXT.md` reflect the updated transition matrix.
10. All existing handler unit tests and integration tests pass.
11. New unit tests cover the `ActivateArrangementHandler` (happy path, wrong status, row-version mismatch, not found). `StartArrangementHandlerTests` is updated to assert rejection when status is `Created`.

## Implementation Plan

### Backend

1. **Add `Active` to the domain enum**
   - `src/Backend/Tasting.Api/Features/Arrangement/Domain/ArrangementStatus.cs` — add `Active` between `Created` and `Started`.

2. **Add `Active` to the contracts enum**
   - `src/Backend/Tasting.Api/Contracts/ArrangementStatus.cs` — add `Active` between `Created` and `Started`.

3. **Create the ActivateArrangement vertical slice**
   - Create folder `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/ActivateArrangement/`
   - `ActivateArrangementRequest.cs` — `record(uint RowVersion)`
   - `ActivateArrangementCommand.cs` — `record(Guid ArrangementId, uint RowVersion) : IRequest<Domain.Arrangement>`
   - `ActivateArrangementMapper.cs` — maps `Domain.Arrangement` → `ArrangementResponse`
   - `ActivateArrangementHandler.cs` — fetches arrangement, validates `Status == Created`, validates `RowVersion`, sets `Status = Active`, increments `RowVersion`, saves. Throws `ServiceNotFoundException` (404), `ConflictException` (409) as appropriate.
   - `ActivateArrangementEndpoint.cs` — `POST /arrangements/{arrangementId}/activate`, tag `Arrangements`, role `Admin`.

   Pattern to follow: mirror `StartArrangement` slice exactly (same structure: `BaseCommandEndpoint<TReq, TRes, TCmd, TDomain, TMapper>`).

4. **Update `StartArrangementHandler`**
   - `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/StartArrangement/StartArrangementHandler.cs`
   - Change guard from `ArrangementStatus.Created` to `ArrangementStatus.Active`.
   - Update the error message: `"Arrangement cannot be started from status '{arrangement.Status}'. Only 'Active' arrangements can be started."`.

### Frontend

5. **Add `Active` to frontend enum**
   - `src/Frontend/Tasting.Admin/Features/Arrangement/Models/ArrangementModels.cs`
   - Add `Active = 4` (or maintain consistent int ordering with the backend enum).

6. **Add `ActivateAsync` to `IArrangementsApiClient`**
   - Signature: `Task<ArrangementDto?> ActivateAsync(Guid id, uint rowVersion);`

7. **Implement `ActivateAsync` in `ArrangementsApiClient`**
   - `POST /api/v1/arrangements/{id}/activate` with body `{ rowVersion }`.

8. **Fix `ChangeStatusAsync` mismatch**
   - Remove or replace the broken `POST /api/v1/arrangements/{id}/change-status` call with the correct dedicated endpoint(s) where it is used in pages/components. If `ChangeStatusAsync` is no longer needed after adding the dedicated methods, mark it obsolete or remove it.

### Docs

9. **Update ADR 0003**
   - `docs/adr/0003-arrangement-status-transitions.md`
   - Replace the old transition matrix with: `Created → Active`, `Active → Started`, `Started → Completed`, `Created → Canceled`, `Canceled → Created`.
   - Document that `Active` is one-way (no rollback to `Created`). `Active` is not cancellable.

10. **Update CONTEXT.md**
    - `CONTEXT.md` — update the `Arrangement status` definition to: `Created → Active → Started → Completed`, or `Created → Canceled`. Include that `Active` is the confirmed/locked-in staging state before rating opens.

### Tests

11. **New unit tests: `ActivateArrangementHandlerTests`**
    - `tests/Tasting.Api.UnitTests/Arrangement/ActivateArrangementHandlerTests.cs`
    - Happy path: arrangement transitions to `Active`, `RowVersion` increments.
    - Conflict: wrong status (e.g. `Started`) → `ConflictException`.
    - Conflict: row-version mismatch → `ConflictException`.
    - Not found: unknown `arrangementId` → `ServiceNotFoundException`.

12. **Update `StartArrangementHandlerTests`**
    - `tests/Tasting.Api.UnitTests/Arrangement/StartArrangementHandlerTests.cs`
    - `HandleAsync_TransitionsToStarted_AndTakesSnapshots` — seed arrangement with `Active` status (not `Created`).
    - `HandleAsync_ThrowsConflict_WhenNotInCreatedStatus` — rename/update to `_WhenNotInActiveStatus`; seed with `Created` status to assert rejection.

13. **Update integration tests**
    - `tests/Tasting.Api.IntegrationTests/Arrangement/ArrangementEndpointsTests.cs`
    - Add a test that activates, then starts an arrangement (full happy-path chain through `Created → Active → Started`).
    - Add a test that `start` is rejected when arrangement is still `Created`.

## Files to Change

| File | What to change |
|------|----------------|
| `src/Backend/Tasting.Api/Features/Arrangement/Domain/ArrangementStatus.cs` | Add `Active` |
| `src/Backend/Tasting.Api/Contracts/ArrangementStatus.cs` | Add `Active` |
| `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/ActivateArrangement/` | Create new vertical slice (5 files) |
| `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/StartArrangement/StartArrangementHandler.cs` | Change guard to `Active`, update error message |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Models/ArrangementModels.cs` | Add `Active` to frontend enum |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Services/ArrangementsApiClient.cs` | Add `ActivateAsync`, fix `ChangeStatusAsync` mismatch |
| `docs/adr/0003-arrangement-status-transitions.md` | Update transition matrix |
| `CONTEXT.md` | Update `Arrangement status` definition |
| `tests/Tasting.Api.UnitTests/Arrangement/ActivateArrangementHandlerTests.cs` | Create new test file |
| `tests/Tasting.Api.UnitTests/Arrangement/StartArrangementHandlerTests.cs` | Update guards to expect `Active` |
| `tests/Tasting.Api.IntegrationTests/Arrangement/ArrangementEndpointsTests.cs` | Add activate + chain tests |

## Tests

- **Unit:** `Tasting.Api.UnitTests` project — new `ActivateArrangementHandlerTests.cs`, updated `StartArrangementHandlerTests.cs`.
- **Integration:** `Tasting.Api.IntegrationTests` project — updated `ArrangementEndpointsTests.cs`.
- Run: `dotnet test tests/Tasting.Api.UnitTests` and `dotnet test tests/Tasting.Api.IntegrationTests`.

## Out of Scope

- No DB migration needed — status is stored as string; adding the `Active` enum value is sufficient.
- No `Active → Created` rollback.
- `Active` arrangements are not cancellable.
- No changes to rating, results, catalog, or identity features.

## Domain Terms

- **`Active` (Arrangement status):** Confirmed and locked-in staging state. Setup (beers, participants) is frozen. The arrangement is committed to happening but the rating window has not yet opened. One-way — no rollback to `Created`. See `CONTEXT.md` for full glossary.

## Suggested Skills for Implementing Agent

- `tdd` — implement `ActivateArrangementHandler` test-first.
- `code-review` — after implementation, verify the status guard changes are consistent across all handlers.

## Handoff Prompt

You are implementing issue #44 "Arrangement Status" in the Tasting repository.

Read the full spec at `docs/handoff/issue-44-arrangement-status.md` before doing anything.

Work on a dedicated branch with prefix `codex/`. 

Your task is to add an `Active` arrangement status and a new `ActivateArrangement` vertical slice, update the `StartArrangement` guard from `Created` to `Active`, update the frontend API client, fix the `ChangeStatusAsync` mismatch, and update ADR 0003 + CONTEXT.md. Follow the exact implementation plan in the spec. After all changes are complete, run:

```
dotnet test tests/Tasting.Api.UnitTests
dotnet test tests/Tasting.Api.IntegrationTests
```

Fix any failures before opening a PR. The PR title should be: `feat: add Active arrangement status (#44)`. Link the issue in the PR body but do NOT close it.
