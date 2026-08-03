# 0030. Optimistic concurrency on rating updates

**Status:** Accepted

## Context
Samtidige writes på samme rating kan ellers gi siste-write-vinner uten kontroll og skjulte overskrivinger.

## Decision
Rating-rader skal bruke optimistic concurrency med versjonsfelt.
Tapende concurrent update returnerer `409 Conflict`.

## Consequences
- Forutsigbar konfliktatferd ved parallelle rating-endringer.
- Klient må håndtere refresh/retry-flyt ved `409`.
- Krever versjonskolonne og betinget oppdateringsmønster i PostgreSQL.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
