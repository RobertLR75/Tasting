# 0032. API versioning from day one

**Status:** Accepted

## Context
Kontrakten vil sannsynligvis utvikles (paginering, filtrering, payload-justeringer), og klientkompatibilitet må beskyttes.

## Decision
API-et versjoneres fra start med `/api/v1/...`.

## Consequences
- Gir kontrollert evolusjon uten å bryte eksisterende klienter.
- Krever konsekvent routing- og dokumentasjonsstrategi per versjon.
- Forenkler introduksjon av `v2` når kontraktsendringer blir nødvendige.
