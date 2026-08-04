# 0003. Arrangement status transition matrix

**Status:** Accepted

## Context
Statusregler måtte konkretiseres for å kunne validere oppdatering og beskytte domeneinvarianter.

## Decision
Tillatte overganger:
- `Created -> Started`
- `Created -> Canceled`
- `Canceled -> Created`
- `Started -> Completed`

Ikke tillatt:
- `Started -> Created`
- Alle overganger fra `Completed`
- `Started -> Canceled`
- `Canceled -> Started`
- `Canceled -> Completed`

## Consequences
- Endepunkter må avvise ulovlige transitions med tydelig feilkode.
- Status kan behandles som enkel state machine med deterministisk validering.
- Kansellering blir reverserbar til samme arrangementutkast i stedet for terminal sluttilstand.
- Gjenåpning fra `Canceled` til `Created` må bevare eksisterende beers og participants.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
