# 0031. Unified error contract across all endpoints

**Status:** Accepted

## Context
Ulike feilformater gjør klienthåndtering kompleks og inkonsistent.

## Decision
Alle endepunkter bruker samme feilkontrakt for minst `400`, `403`, `404`, `409`, med felter for:
- `code`
- `message`
- `correlationId`

## Consequences
- Forenkler klientlogikk og observability.
- Krever konsistent mapping fra domene/valideringsfeil til standardiserte feilkoder.
- Bør implementeres sentralt i endpoint/shared library-lag.
