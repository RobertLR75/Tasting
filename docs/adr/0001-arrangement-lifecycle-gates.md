# 0001. Arrangement lifecycle gates for membership changes

**Status:** Proposed

## Context
API-et skal støtte oppretting, lesing og oppdatering av arrangement. Det skal også være mulig å legge til beers og participants, men bare når arrangement ikke er startet.

## Decision
`Arrangement` modelleres som aggregate root med tydelige livssyklusregler:
- `Created`: tillater oppdatering av metadata og adding av beers/participants
- `Started`: låser medlemskap (ingen nye beers/participants)
- `Canceled` og `Completed`: låser medlemskap og vanlige oppdateringer

Endepunkter som muterer medlemskap må validere status før endring.

## Consequences
- Forutsigbar domenelogikk og færre inkonsistente data.
- Klient får eksplisitte valideringsfeil ved ulovlige statusoverganger eller mutations.
- Krever at statusregler sentraliseres i `IRequestHandler`-laget, ikke spres i endepunkter.
- Skal verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
