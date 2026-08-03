# 0013. Membership removal allowed only in Created

**Status:** Accepted

## Context
Det måtte avklares om participants/beers kan fjernes, og når.

## Decision
- `RemoveParticipant` og `RemoveBeer` er tillatt i `Created`.
- Etter `Started` er arrangement immutable, og remove er ikke tillatt.

## Consequences
- API må validere status før remove-operasjoner.
- Forsøk på remove når status ikke er `Created` avvises (konflikt).
- Klientflyt blir konsistent med øvrige medlemskapsregler.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
