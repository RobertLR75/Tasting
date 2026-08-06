# Spec: Wireframe for Frontend (Issue #42)

**Issue:** https://github.com/RobertLR75/Tasting/issues/42  
**Date:** 2026-08-06  
**Status:** Ready for development

---

## Problem

The participant-facing frontend (`Tasting.App`) has no wireframes. Developers cannot implement the 9-screen tasting flow — login → join arrangement → rate beers → view results — without a shared UI contract that expresses backend domain rules visually.

---

## Acceptance Criteria

1. A Markdown wireframe document `docs/wireframes-participant.md` exists with one ASCII wireframe section per screen (9 screens), each annotated with state rules, backend errors, and SignalR triggers.
2. A draw.io file `docs/wireframes-participant.drawio` exists with all 9 screens modelled as individual diagram pages, consistent with the Markdown wireframes.
3. Both files are added as `SolutionItems` under the `Docs` solution folder in `Tasting.sln`.
4. The wireframes cover the full tasting loop: screens 5–7 repeat once per beer in the arrangement.
5. The "Next" button on the Rating screen (screen 6) is disabled until all four sliders (Visibility, Smell, Taste, Toast) have a value set.
6. The "Next" button on the Beer Results screen (screen 7) is disabled until all participants have submitted their rating, detected via SignalR.
7. The Lobby (screen 4) blocks navigation until the Arrangement transitions from `Created` → `Started`, detected via SignalR.
8. The wireframes reference existing domain terminology from `CONTEXT.md` (Rating, Arrangement, Participant, Beer, BeerStyle, BeerType, Result).
9. An ADR (`docs/adr/0046-participant-wireframes-live-in-separate-document.md`) records the decision to keep participant wireframes separate from the admin backoffice wireframes.

---

## Implementation Plan

1. **Create ADR 0046** at `docs/adr/0046-participant-wireframes-live-in-separate-document.md` documenting the decision.
2. **Create `docs/wireframes-participant.md`** — 9 sections (see template below), following the same format as `docs/wireframes.md`.
3. **Create `docs/wireframes-participant.drawio`** — 9 pages, one per screen. Use draw.io's built-in wireframe shape library (`mxgraph.ios7` or generic wireframe shapes). Each page title should match the screen name (e.g. "1. Login", "2. Create User", etc.).
4. **Update `Tasting.sln`** — add both new files as `SolutionItems` in the existing `Docs` folder (after the last existing entry).
5. **Commit** with message: `docs: add participant wireframes for issue #42`.

---

## Screens

### Screen 1 — Login

| | |
|---|---|
| **Route** | `/login` |
| **Shell** | None (standalone screen) |
| **Fields** | E-post (text), Passord (password) |
| **Buttons** | Logg inn |
| **Links** | Opprett bruker → screen 2 |
| **State rules** | "Logg inn" enabled when both fields non-empty |
| **Backend errors** | Any rejection (unknown e-post, wrong password, inactive user) → generic «Ugyldig e-post eller passord.» — no reason disclosed |
| **SignalR** | — |

### Screen 2 — Create User

| | |
|---|---|
| **Route** | `/register` |
| **Shell** | None |
| **Fields** | E-post, Fornavn, Etternavn, Passord |
| **Buttons** | Opprett, Avbryt (→ back to Login) |
| **State rules** | "Opprett" enabled when all fields non-empty; username = e-post |
| **Backend errors** | Duplicate e-post → «E-posten er allerede i bruk.»; validation failures → field-level messages |
| **SignalR** | — |

### Screen 3 — List Arrangements

| | |
|---|---|
| **Route** | `/arrangements` |
| **Shell** | App shell with top bar |
| **Content** | Grid: Navn, Dato, Status. Only `Created` and `Started` arrangements shown. |
| **Links** | «Bli med» → Lobby (screen 4), only for `Created` arrangements the user has been added to as a Participant |
| **State rules** | Empty list → «Ingen aktive arrangementer.» |
| **Backend errors** | Load failure → generic error message |
| **SignalR** | — |

### Screen 4 — Arrangement Lobby

| | |
|---|---|
| **Route** | `/arrangements/{id}/lobby` |
| **Shell** | App shell |
| **Content** | Grid: Navn, Bryggeri, Stil — beers in the arrangement. Melding: «Venter på at arrangementet starter…» |
| **Buttons** | — (no action until started) |
| **State rules** | Navigation to screen 5 (first beer) is triggered **automatically** when SignalR signals `Arrangement.Status → Started`; user cannot advance manually |
| **Backend errors** | Load failure → generic error message |
| **SignalR** | Subscribe to `ArrangementStatusChanged`; on `Started` → navigate to screen 5 for beer[0] |

### Screen 5 — Beer Info (loop, beer index 1–N)

| | |
|---|---|
| **Route** | `/arrangements/{id}/beers/{beerIndex}` |
| **Shell** | App shell |
| **Content** | Bilde (if available), Navn, Beskrivelse, Bryggeri, Stil, Type. Progress indicator: «Øl X av N» |
| **Buttons** | Neste → screen 6 (Rating for same beer) |
| **State rules** | "Neste" always enabled |
| **Backend errors** | Load failure → generic error message |
| **SignalR** | — |

### Screen 6 — Rating for Beer X (loop)

| | |
|---|---|
| **Route** | `/arrangements/{id}/beers/{beerIndex}/rate` |
| **Shell** | App shell |
| **Content** | 4 labelled sliders: Utseende (0–10, step 0.5), Lukt (0–10, step 0.5), Smak (0–10, step 0.5), Skål (0–10, step 0.5). Each slider shows current value. |
| **Buttons** | Neste → screen 7 |
| **State rules** | "Neste" disabled until all 4 sliders have been moved from their default/null state (i.e. user has set an explicit value for each). Submission sent when "Neste" clicked. |
| **Backend errors** | Rating submit failure → inline error; optimistic concurrency conflict (409) → «Ratingen ble oppdatert av en annen instans — last siden på nytt.» |
| **SignalR** | — |

### Screen 7 — Results for Beer X (loop)

| | |
|---|---|
| **Route** | `/arrangements/{id}/beers/{beerIndex}/results` |
| **Shell** | App shell |
| **Content** | Din totale rating (the user's own TotalRating for this beer). Total rating for alle deltakere (Result score, 2 decimals). Melding: «Venter på at alle deltakere avgir sin rating…» (shown while waiting) |
| **Buttons** | Neste → screen 5 for next beer, or screen 8 if last beer |
| **State rules** | "Neste" disabled until all participants have submitted a rating, signalled via SignalR |
| **Backend errors** | Load failure → generic error |
| **SignalR** | Subscribe to `AllRatingsSubmittedForBeer`; on event → enable "Neste" |

### Screen 8 — Result for Arrangement

| | |
|---|---|
| **Route** | `/arrangements/{id}/results` |
| **Shell** | App shell |
| **Content** | Grid: Rangering (1,2,3…), Øl-navn, Bryggeri, Total rating (Result score). Sorted by Result score descending (tie-breakers: most ratings, lowest std-dev, BeerId asc). |
| **Links** | «Åpne øl» → screen 9 for that beer |
| **Buttons** | Ferdig → screen 3 (back to List Arrangements) |
| **State rules** | Grid always shown (arrangement is Completed at this point) |
| **Backend errors** | Load failure → generic error |
| **SignalR** | — |

### Screen 9 — Beer X with Result

| | |
|---|---|
| **Route** | `/arrangements/{id}/results/beers/{beerId}` |
| **Shell** | App shell |
| **Content** | Bilde, Navn, Total rating, Beskrivelse, Bryggeri, Stil, Type |
| **Buttons** | Lukk → back to screen 8 |
| **State rules** | — |
| **Backend errors** | Load failure → generic error |
| **SignalR** | — |

---

## Files to Change

| File | What to change |
|---|---|
| `docs/wireframes-participant.md` | **Create** — full ASCII wireframe document (9 screens, same format as `docs/wireframes.md`) |
| `docs/wireframes-participant.drawio` | **Create** — draw.io file with 9 diagram pages |
| `docs/adr/0046-participant-wireframes-live-in-separate-document.md` | **Create** — ADR recording the decision |
| `Tasting.sln` | Add both new docs files as `SolutionItems` in the `Docs` folder |

---

## Tests

No automated tests apply to wireframe documents. The implementing agent should:
- Verify `docs/wireframes-participant.md` renders correctly in a Markdown viewer.
- Verify `docs/wireframes-participant.drawio` opens without error in draw.io (desktop or app.diagrams.net).

---

## Out of Scope

- Implementing any Ionic React / TypeScript code for `Tasting.App` — that is a separate issue.
- Navigation routing implementation.
- SignalR hub implementation.
- Authentication/JWT wiring.
- The admin backoffice (`Tasting.Admin`) — covered by existing wireframes.

---

## Domain Terms

All terms are as defined in `CONTEXT.md`. Key references for this spec:

| Term | Meaning |
|---|---|
| **Arrangement** | A tasting session; statuses `Created → Started → Completed` |
| **Participant** | A User added to an Arrangement who may submit Ratings |
| **Beer snapshot** | Frozen beer metadata at Arrangement start — displayed in screens 4, 5, 7, 8, 9 |
| **Rating** | 4-field score (Visibility, Smell, Taste, Toast) 0–10 step 0.5; TotalRating server-computed |
| **Result** | Aggregated score per (Arrangement, Beer); frozen at Completed |
| **Result score** | Average of all participant ratings for a beer, rounded to 2 decimals |

---

## Suggested Skills for Implementing Agent

- `grill-me_dotnet_structure_guide` — for project structure reference  
- `research` — if draw.io XML format details are needed  
- `code-review` — after wireframe documents are created

---

## Handoff Prompt

```
Read the spec at docs/handoff/issue-42-wireframe-for-frontend.md and implement it on this branch.

Tasks:
1. Create docs/adr/0046-participant-wireframes-live-in-separate-document.md (decision: separate from admin wireframes).
2. Create docs/wireframes-participant.md with 9 ASCII wireframe sections following the same format as docs/wireframes.md.
   Screens: Login, Create User, List Arrangements, Lobby (SignalR), Beer Info, Rating (4×slider), Beer Results (SignalR), Arrangement Results, Beer Detail with Result.
3. Create docs/wireframes-participant.drawio with 9 diagram pages (one per screen), using draw.io wireframe shapes.
4. Add both new files (wireframes-participant.md and wireframes-participant.drawio) as SolutionItems in the Docs folder in Tasting.sln.
5. Commit with: docs: add participant wireframes for issue #42
6. Open a PR linked to issue #42.

Issue: https://github.com/RobertLR75/Tasting/issues/42
Branch behavior: commit directly to the current feature branch.
Do NOT close the issue.
```
