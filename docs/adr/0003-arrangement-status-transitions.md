# 0003. Arrangement status transition matrix

**Status:** Accepted

## Context
Statusregler måtte konkretiseres for å kunne validere oppdatering og beskytte domeneinvarianter.

## Decision
Tillatte overganger:
- `Created -> Active` (bekreftelse/låsing; beers og participants fryses)
- `Active -> Started` (åpner rating-vindu)
- `Started -> Completed`
- `Created -> Canceled`
- `Canceled -> Created`

Ikke tillatt:
- `Active -> Created` (Active er enveis; ingen rollback)
- `Active -> Canceled` (kun Created kan kanselleres)
- `Started -> Created`
- Alle overganger fra `Completed`
- `Started -> Canceled`
- `Canceled -> Started`
- `Canceled -> Completed`

## Consequences
- Endepunkter må avvise ulovlige transitions med tydelig feilkode.
- Status kan behandles som enkel state machine med deterministisk validering.
- `Active` er bekreftelsessteg som låser arrangement-innhold (beers, participants, navn/beskrivelse).
- `StartArrangement` krever nå `Active`-status; et `Created`-arrangement må aktiveres først.
- Kansellering er fortsatt reverserbar fra `Created`; `Active`-arrangements kan ikke kanselleres.
- Gjenåpning fra `Canceled` til `Created` bevarer eksisterende beers og participants.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
