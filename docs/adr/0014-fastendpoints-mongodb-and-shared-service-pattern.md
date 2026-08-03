# 0014. FastEndpoints + MongoDB with shared service interface pattern

**Status:** Accepted

## Context
Nye API-endepunkter for Beer, Rating og Results skal implementeres i samme løsning og må følge valgt teknologistack og eksisterende abstrahering.

## Decision
- Endepunkter bygges med FastEndpoints.
- Data persisteres i MongoDB.
- Løsningen følger `IService`-mønster via `SharedLibrary.FastEndpoints` og `SharedLibrary.MongoDB`.

## Consequences
- Domenelogikk kan holdes samlet i service-lag med konsistente endpoint-kontrakter.
- Persistens og endpoint-mønster blir likt på tvers av slices.
- Krever tydelig avgrensning mellom domain/service/endpoint for å unngå logikklekkasje i endpoint-klasser.
