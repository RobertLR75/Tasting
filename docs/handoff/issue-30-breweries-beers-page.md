# Spec: Breweries / Beers page (Issue #30)

**Issue:** https://github.com/RobertLR75/Tasting/issues/30
**Date:** 2026-08-06
**Status:** Ready for development

## Problem

Navigating to `/breweries/{id}/beers` shows all beers in the catalog instead of only the beers belonging to that brewery. The frontend calls `GET /api/v1/beers?breweryId={id}`, but the backend `ListBeersRequest` has no `BreweryId` property and `ListBeersHandler` never filters by brewery — so the query parameter is silently ignored. Additionally, `AddBeerPage` hardcodes `Guid.Empty` for `BeerStyleId` and `BeerTypeId`, making beer creation non-functional.

## Acceptance Criteria

1. A new endpoint `GET /api/v1/breweries/{id}/beers` exists and returns only beers belonging to the specified brewery.
2. The endpoint returns HTTP 404 when the brewery ID does not exist.
3. The endpoint returns HTTP 200 with a filtered beer list when the brewery exists (even if the list is empty).
4. `BeersApiClient.ListAsync` in the frontend calls the new nested route `/api/v1/breweries/{id}/beers` instead of `/api/v1/beers?breweryId={id}`.
5. `BeersPage.razor` fetches the brewery by ID on load and displays the brewery name in the page header (e.g. "Beers — Mack Brewery").
6. `AddBeerPage.razor` offers dropdown selectors for BeerStyle and BeerType (loaded from existing API clients) so a beer can be fully created with a valid `BeerStyleId` and `BeerTypeId`.
7. All existing unit and integration tests continue to pass.

## Implementation Plan

### Backend

1. **Create vertical slice `Features/Catalog/Breweries/Beers/ListBreweryBeers/`** with:
   - `ListBreweryBeersRequest.cs` — `record ListBreweryBeersRequest(Guid BreweryId)`
   - `ListBreweryBeersQuery.cs` — `record ListBreweryBeersQuery(Guid BreweryId) : IRequest<ListBreweryBeersResult>`
   - `ListBreweryBeersResult.cs` — reuse `ListBeersItem` record from `ListBeers` (or define a local equivalent)
   - `ListBreweryBeersHandler.cs` — check brewery exists (throw `ServiceNotFoundException` if not), filter `dbContext.Beers` by `BreweryId`, join names via separate dictionary lookups (same pattern as `ListBeersHandler`)
   - `ListBreweryBeersResponse.cs` — identical shape to `ListBeersResponse`
   - `ListBreweryBeersMapper.cs` — map query/result to response
   - `ListBreweryBeersEndpoint.cs` — `Get("/breweries/{breweryId:guid}/beers")`, tags "Beers", roles Admin + User

### Frontend

2. **Update `CatalogApiClients.cs`**:
   - Add `IBeerStylesApiClient` and `IBeerTypesApiClient` interfaces + implementations calling `GET /api/v1/beer-styles` and `GET /api/v1/beer-types` respectively (check actual route from existing endpoints).
   - Update `IBeersApiClient.ListAsync` to accept a required `Guid breweryId` and call `/api/v1/breweries/{breweryId}/beers` (drop `?breweryId=` approach).
   - Register new API clients in `Program.cs`.

3. **Update `BeersPage.razor`**:
   - On `OnInitializedAsync`, fetch the brewery by calling `BreweriesApiClient.GetAsync(BreweryId)`.
   - Update page header to show `"Beers — {brewery.Name}"` when brewery is loaded.
   - Update `BeersApiClient.ListAsync` call signature to pass `BreweryId` as required parameter.

4. **Update `AddBeerPage.razor`**:
   - Inject `IBeerStylesApiClient` and `IBeerTypesApiClient`.
   - On init, load beer styles and beer types.
   - Replace the static `Guid.Empty` with `MudSelect` dropdowns for BeerStyle and BeerType.
   - Validate that both dropdowns have a selection before submitting.

### Tests

5. **Add unit test `ListBreweryBeersHandlerTests.cs`** in `tests/Tasting.Api.UnitTests/Catalog/`:
   - `HandleAsync_ReturnsOnlyBeersForBrewery` — seeds two breweries with beers each; asserts only the requested brewery's beers are returned.
   - `HandleAsync_ThrowsNotFound_WhenBreweryDoesNotExist` — asserts `ServiceNotFoundException` is thrown.

## Files to Change

| File | What to change |
|------|---------------|
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersRequest.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersQuery.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersResult.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersHandler.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersResponse.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersMapper.cs` | New file |
| `src/Backend/Tasting.Api/Features/Catalog/Breweries/Beers/ListBreweryBeers/ListBreweryBeersEndpoint.cs` | New file |
| `src/Frontend/Tasting.Admin/Features/Catalog/Services/CatalogApiClients.cs` | Add `IBeerStylesApiClient`, `IBeerTypesApiClient`; update `BeersApiClient.ListAsync` to new route |
| `src/Frontend/Tasting.Admin/Features/Catalog/Models/CatalogModels.cs` | Add `BeerStyleDto`, `BeerTypeDto`, `ListBeerStylesResponse`, `ListBeerTypesResponse` if missing |
| `src/Frontend/Tasting.Admin/Program.cs` | Register new API clients |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/BeersPage.razor` | Fetch brewery name; update header; fix client call |
| `src/Frontend/Tasting.Admin/Features/Catalog/Pages/AddBeerPage.razor` | Add BeerStyle and BeerType dropdowns |
| `tests/Tasting.Api.UnitTests/Catalog/ListBreweryBeersHandlerTests.cs` | New file |

## Tests

- `tests/Tasting.Api.UnitTests/Catalog/ListBreweryBeersHandlerTests.cs` — handler unit tests (filter by brewery, 404 on missing brewery)
- Run existing `tests/Tasting.Api.UnitTests` to ensure nothing regressed
- Run existing `tests/Tasting.Api.IntegrationTests` if available

## Out of Scope

- Modifying the existing `GET /api/v1/beers` (flat list) endpoint — leave it unchanged
- EditBeerPage BeerStyle/BeerType dropdown (not referenced in the issue)
- Pagination on the brewery beers list

## Domain Terms

- **Brewery**: A producer of beers. Every Beer must belong to exactly one Brewery (`Beer.BreweryId` is required).
- **Beer**: A catalog item produced by a Brewery. Has a BeerStyle and BeerType.
- **ListBreweryBeers**: The scoped listing of beers within a given brewery context.

See `docs/glossary.md` for the full glossary.

## Suggested Skills for Implementing Agent

- `tdd` — implement handler tests first, then the handler
- `code-review` — review against coding standards after implementation

## Handoff Prompt

```
Read docs/handoff/issue-30-breweries-beers-page.md in full before writing any code.

Issue: https://github.com/RobertLR75/Tasting/issues/30

Work on branch: codex/<your-branch-name> branched from main.

Summary of work:
1. Add a new vertical slice GET /api/v1/breweries/{id}/beers (returns 404 if brewery not found).
   Follow the existing pattern in Features/Catalog/Breweries/GetBrewery/ and Features/Catalog/Beers/ListBeers/.
   Place new files under Features/Catalog/Breweries/Beers/ListBreweryBeers/.
2. Update BeersApiClient.ListAsync in CatalogApiClients.cs to call the new nested route.
3. Update BeersPage.razor to load and display the brewery name in the header.
4. Update AddBeerPage.razor to load BeerStyle and BeerType from their API endpoints and render
   MudSelect dropdowns, replacing the Guid.Empty placeholders.
5. Add unit tests in tests/Tasting.Api.UnitTests/Catalog/ListBreweryBeersHandlerTests.cs.
6. Run dotnet test to verify all tests pass.
7. Open a PR linking to issue #30.
```
