# 0007. Arrangement is immutable after Started

**Status:** Accepted

## Context
Oppdateringsregler for arrangementfelter måtte konkretiseres for å unngå uklare writes i aktiv/avsluttet flyt.

## Decision
Bare arrangement med status `Created` er redigerbare. `Started`, `Completed` og `Canceled` er ikke redigerbare for navn, dato eller beskrivelse.

## Consequences
- `UpdateArrangement` må avvise endringer når status ikke er `Created`.
- Reduserer risiko for at grunnlag endres mens rating pågår.
- `Canceled` oppfører seg som et låst, men reverserbart utkast via separat statusendring tilbake til `Created`.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
