# 0028. V1 list endpoints return full lists (no pagination)

**Status:** Accepted

## Context
Det måtte avklares om liste-endepunkter skal bygges med paginering/filtrering i første versjon.

## Decision
I v1 returnerer liste-endepunkter full liste uten paginering.

## Consequences
- Raskere implementasjon i første leveranse.
- Økt risiko for større payloads ved vekst i data.
- Trenger plan for senere introduksjon av paginering uten å bryte klientkontrakter.
