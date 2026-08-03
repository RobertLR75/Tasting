# 0006. Optimistic concurrency for arrangement mutations

**Status:** Accepted

## Context
Parallelle kall kan konkurrere: samtidig `AddParticipant`/`AddBeer` mens arrangement startes. Uten concurrency-kontroll kan klienter få inkonsistent atferd.

## Decision
Bruk optimistic concurrency på `Arrangement` (f.eks. `RowVersion`/ETag).
- Muterende kall (`StartArrangement`, `AddParticipant`, `AddBeer`, oppdatering) må skrive mot kjent versjon.
- Første commit vinner.
- Tapende request returnerer `409 Conflict`.
- Hvis status allerede er blitt `Started`, returneres `409 Conflict` med tydelig domeneårsak.

## Consequences
- Deterministisk konfliktatferd under konkurrerende writes.
- Klient må håndtere retry/reload på `409`.
- Krever versjonsfelt i persistens + mapping til API-kontrakt.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
