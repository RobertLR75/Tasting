# 0005. Rating and results are gated by arrangement status

**Status:** Proposed

## Context
Det var nødvendig å fastsette når rating kan utføres, og om resultater kan etterregistreres.

## Decision
- `Rating` er kun tillatt når arrangement er i status `Started`.
- `Results` kan ikke etterregistreres.

## Consequences
- Rating-endepunkt må validere status `Started` før mutasjon.
- Resultatlogikk må avvise writes utenfor gyldig flyt.
- Trenger presis mapping av feilstatus (f.eks. `409` vs `422`) for ulovlig tidspunkt.
