# 0006. Optimistic concurrency for arrangement mutations

**Status:** Accepted

## Context
Parallelle kall kan konkurrere om samme arrangement. Uten concurrency-kontroll kan en tapende write overskrive en vinnende write. Klientstyrt `RowVersion` lekket denne persistensdetaljen inn i domenemodell, API-kontrakter og frontend.

## Decision
Bruk backend-intern optimistic concurrency på arrangementets persistensmodell.
- `Arrangement` er en ren domenemodell uten versjonsfelt.
- Persistensmodellen mapper eksplisitt til domenet og beholder eksisterende `row_version` som concurrency token.
- Backend leser gjeldende persistensversjon og lar databasen oppdage endringer mellom lesing og commit.
- Første commit vinner.
- Tapende request returnerer `409 Conflict` gjennom `Unified error contract`; backend prøver ikke automatisk på nytt.
- Hvis status allerede er blitt `Started`, returneres `409 Conflict` med tydelig domeneårsak.

## Consequences
- Deterministisk konfliktatferd under konkurrerende writes.
- Klienter lagrer eller sender ingen versjonsverdi; felles feilhåndtering viser API-meldingen og henter ferske data på `409`.
- En bruker som lagrer fra en gammel skjermvisning oppdages ikke dersom ingen konkurrerende write skjer mellom backendens lesing og commit. Dette er en akseptert konsekvens.
- Concurrency verifiseres med separate persistensspor mot samme rad og API-integrasjonstester for konfliktkontrakten.
