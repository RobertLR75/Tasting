# 0003. Arrangement status transition matrix

**Status:** Proposed

## Context
Statusregler måtte konkretiseres for å kunne validere oppdatering og beskytte domeneinvarianter.

## Decision
Tillatte overganger:
- `Created -> Started`
- `Created -> Canceled`
- `Started -> Completed`

Ikke tillatt:
- `Started -> Created`
- Alle overganger fra `Completed`
- Alle overganger fra `Canceled`

## Consequences
- Endepunkter må avvise ulovlige transitions med tydelig feilkode.
- Status kan behandles som enkel state machine med deterministisk validering.
- Trenger avklaring av hvilken rolle/autorisasjon som kan utføre hver gyldig overgang.
