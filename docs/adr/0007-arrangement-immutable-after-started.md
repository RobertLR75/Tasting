# 0007. Arrangement is immutable after Started

**Status:** Accepted

## Context
Oppdateringsregler for arrangementfelter måtte konkretiseres for å unngå uklare writes i aktiv/avsluttet flyt.

## Decision
Når arrangement har status `Started`, er arrangementet ikke lenger redigerbart.

## Consequences
- `UpdateArrangement` må avvise endringer når status er `Started`.
- Reduserer risiko for at grunnlag endres mens rating pågår.
- Krever avklaring av hva som gjelder i `Canceled` og `Completed` for konsistent regelsett.
