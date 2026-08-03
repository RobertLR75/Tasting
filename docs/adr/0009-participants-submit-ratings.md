# 0009. Ratings are submitted by participants only

**Status:** Accepted

## Context
Tilgang for rating måtte avgrenses fra admin-mutasjoner av arrangement.

## Decision
Kun participants kan legge inn ratings.

## Consequences
- Rating-endepunkt må verifisere at innlogget bruker er participant i aktuelt arrangement.
- Ikke-participants (inkludert admin uten participant-rolle) får `403 Forbidden`.
- Krever tydelig identitetskobling mellom `User` og arrangement-participant.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
