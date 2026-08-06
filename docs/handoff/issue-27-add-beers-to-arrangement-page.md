# Spec: Add beers to Arrangement page (Issue #27)

**Issue:** https://github.com/RobertLR75/Tasting/issues/27  
**Date:** 2026-08-06  
**Status:** Ready for development

## Problem

The "Add Beers to Arrangement" page (`AddBeersPage.razor`) fails with `409 Conflict` every
time the user attempts to add a beer. The root cause is that `AddBeerToArrangementRequest`
(frontend) does not include `RowVersion`, so the API always receives `RowVersion = 0`.
`AddBeerHandler` checks `arrangement.RowVersion != request.RowVersion` and throws
`ConflictException` for any arrangement that has been modified at least once (RowVersion > 0).
A secondary bug in `AddParticipantsPage.razor` always passes `Guid.Empty` as the participant
User ID, so participants can never be correctly added.

## Acceptance Criteria

1. A beer can be added to an arrangement in `Created` status without receiving a 409.
2. Multiple beers can be added in sequence on the same page visit without reloading.
3. The page shows the list of already-added beers (from the arrangement) when it loads.
4. Beers already added to the arrangement are removed from the "Available Beers" search results.
5. `GET /api/v1/arrangements/{id}` response includes a `Beers` collection.
6. `AddBeerToArrangementRequest` (frontend model) includes `RowVersion`.
7. `ArrangementsApiClient.AddBeerAsync` returns `ArrangementDto` (matching what the API actually returns).
8. `AddParticipantsPage.razor` passes the correct User `Guid` (not `Guid.Empty`) when adding a participant.
9. `ApiContractTests` includes a test asserting `AddBeerToArrangementRequest` carries `RowVersion`.
10. `AddBeerHandlerTests` includes a test covering the `RowVersion` mismatch → `ConflictException` path.

## Implementation Plan

1. **Backend — extend `ArrangementResponse` and `GetArrangementMapper`**
   - Add a `Beers` property (e.g. `IReadOnlyList<ArrangementBeerItem>`) to `ArrangementResponse`
     in `src/Backend/Tasting.Api/Features/Arrangement/ArrangementResponse.cs`.
   - Define `ArrangementBeerItem(Guid Id, Guid BeerId, string BeerName)` (or reuse a suitable
     existing record) in the same file or nearby.
   - Update `GetArrangementMapper.FromEntityAsync` to populate `Beers` from `entity.Beers`.
   - `AddBeerMapper.FromEntityAsync` already returns `ArrangementResponse` — it will automatically
     include the updated beers list once the type is extended.

2. **Frontend — update `ArrangementModels.cs`**
   - Add `RowVersion` to `AddBeerToArrangementRequest`:
     ```csharp
     public record AddBeerToArrangementRequest(Guid BeerId, uint RowVersion);
     ```
   - Add a `Beers` collection to `ArrangementDto` to match the extended API response:
     ```csharp
     public record ArrangementDto(Guid Id, string Name, string? Description,
         ArrangementStatus Status, uint RowVersion,
         DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
         IReadOnlyList<ArrangementBeerDto> Beers);
     ```
   - Add a matching `ArrangementBeerItem` record (frontend side):
     ```csharp
     public record ArrangementBeerItem(Guid Id, Guid BeerId, string BeerName);
     ```

3. **Frontend — update `ArrangementsApiClient.cs`**
   - Change `AddBeerAsync` signature to return `ArrangementDto?` instead of `ArrangementBeerDto?`.
   - Update `IArrangementsApiClient` interface accordingly.

4. **Frontend — rewrite `AddBeersPage.razor`**
   - On `OnInitializedAsync`:
     - Call `ArrangementsApiClient.GetAsync(ArrangementId)` to load the arrangement.
     - Store `RowVersion` as a `private uint _rowVersion`.
     - Populate `AlreadyAddedBeerIds` (a `HashSet<Guid>`) from the arrangement's `Beers`.
   - Filter `AvailableBeers` results: exclude beers already in `AlreadyAddedBeerIds`.
   - In `AddBeerAsync(Guid beerId)`:
     - Pass `new AddBeerToArrangementRequest(beerId, _rowVersion)`.
     - On success, update `_rowVersion` from the returned `ArrangementDto.RowVersion`.
     - Add `beerId` to `AlreadyAddedBeerIds` and remove from `AvailableBeers`.
   - Display a second `MudCard` with the already-added beers list (name only, no remove button required by this issue).

5. **Frontend — fix `AddParticipantsPage.razor`**
   - Change `AddingParticipantIds` from `HashSet<int>` to `HashSet<Guid>`.
   - Fix `AddParticipantAsync`:
     - Change parameter to `Guid userId`.
     - Pass `new AddParticipantToArrangementRequest(userId)` (not `Guid.Empty`).
     - Update `AddingParticipantIds.Add(userId)` / `Remove(userId)` accordingly.
     - Update `AvailableUsers.RemoveAll(u => u.Id == userId)` to use `Guid` comparison.
   - Fix the lambda in the Razor template: `OnClick="@(() => AddParticipantAsync(context.Id))"`.

6. **Tests — `Tasting.Admin.UnitTests/ApiContractTests.cs`**
   - Add test: `AddBeerToArrangementRequest_ShouldIncludeRowVersion` — assert the record has
     a `RowVersion` property of type `uint`.

7. **Tests — `Tasting.Api.UnitTests/Arrangement/AddBeerHandlerTests.cs`**
   - Add test: `HandleAsync_RowVersionMismatch_ThrowsConflictException` — set up an arrangement
     with `RowVersion = 5`, issue `AddBeerCommand` with `RowVersion = 3`, assert `ConflictException`.

## Files to Change

| File | What to change |
|------|---------------|
| `src/Backend/Tasting.Api/Features/Arrangement/ArrangementResponse.cs` | Add `Beers` collection property and `ArrangementBeerItem` record |
| `src/Backend/Tasting.Api/Features/Arrangement/Arrangements/GetArrangement/GetArrangementMapper.cs` | Populate `Beers` from entity |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Models/ArrangementModels.cs` | Add `RowVersion` to `AddBeerToArrangementRequest`; add `Beers` to `ArrangementDto`; add `ArrangementBeerItem` record |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Services/ArrangementsApiClient.cs` | Change `AddBeerAsync` return type to `ArrangementDto?` |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddBeersPage.razor` | Load arrangement on init; track `RowVersion`; show already-added beers; filter search results |
| `src/Frontend/Tasting.Admin/Features/Arrangement/Pages/AddParticipantsPage.razor` | Fix `Guid.Empty` bug; fix `HashSet<int>` → `HashSet<Guid>` |
| `tests/Tasting.Admin.UnitTests/ApiContractTests.cs` | Add `AddBeerToArrangementRequest_ShouldIncludeRowVersion` test |
| `tests/Tasting.Api.UnitTests/Arrangement/AddBeerHandlerTests.cs` | Add RowVersion mismatch test |

## Tests

- `tests/Tasting.Admin.UnitTests/ApiContractTests.cs` — assert `AddBeerToArrangementRequest` has `RowVersion : uint`
- `tests/Tasting.Api.UnitTests/Arrangement/AddBeerHandlerTests.cs` — assert `ConflictException` when `command.RowVersion != arrangement.RowVersion`

Run targeted: `dotnet test tests/Tasting.Admin.UnitTests` and `dotnet test tests/Tasting.Api.UnitTests`

## Out of Scope

- Remove-beer button on the "already-added beers" list (not requested)
- Optimistic concurrency UI feedback beyond existing error alerts
- Pagination of the beer search results
- Fixing any other pages beyond `AddBeersPage` and `AddParticipantsPage`

## Domain Terms

- **Optimistic concurrency** — version-based write control; mutations require a matching `RowVersion` (`CONTEXT.md`)
- **Membership uniqueness invariant** — a beer may appear at most once per arrangement (`CONTEXT.md`)
- **Arrangement status** — beers can only be added when status is `Created` (`CONTEXT.md`)

## Suggested Skills for Implementing Agent

- `tdd` — write the new handler test before making handler changes
- `code-review` — review changes against coding standards after implementation

## Handoff Prompt

```
Read the spec at docs/handoff/issue-27-add-beers-to-arrangement-page.md, then implement
issue #27 (https://github.com/RobertLR75/Tasting/issues/27).

Work on a branch named codex/issue-27-add-beers-to-arrangement-page.

Follow every step in the Implementation Plan in order. After all changes are made, run:
  dotnet test tests/Tasting.Admin.UnitTests
  dotnet test tests/Tasting.Api.UnitTests

Open a PR targeting main when the tests pass, linking the issue.
```
