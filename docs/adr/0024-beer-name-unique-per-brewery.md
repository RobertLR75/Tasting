# 0024. Beer name must be unique per Brewery (case-insensitive)

**Status:** Accepted

## Context
Katalogkvalitet krevde tydelig regel for duplikatnavn innen samme brewery.

## Decision
`Beer` skal være unik på `(BreweryId, Name)` case-insensitivt.
Ved konflikt returneres `409 Conflict`.

## Consequences
- MongoDB krever unik indeks med normalisert navn (f.eks. lowercased).
- Forhindrer duplikatoppføringer med kun case-variasjon.
- Klient må håndtere `409` og vise meningsfull feilmelding.
