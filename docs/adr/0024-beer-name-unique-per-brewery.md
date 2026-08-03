# 0024. Beer name must be unique per Brewery (case-insensitive)

**Status:** Accepted

## Context
Katalogkvalitet krevde tydelig regel for duplikatnavn innen samme brewery.

## Decision
`Beer` skal være unik på `(BreweryId, Name)` case-insensitivt.
Ved konflikt returneres `409 Conflict`.

## Consequences
- PostgreSQL krever unik indeks eller constraint med normalisert navn (f.eks. lowercased), opprettet via FluentMigration.
- Forhindrer duplikatoppføringer med kun case-variasjon.
- Klient må håndtere `409` og vise meningsfull feilmelding.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
