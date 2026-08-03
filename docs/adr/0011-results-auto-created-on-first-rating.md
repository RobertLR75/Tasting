# 0011. Results are auto-created on first participant rating

**Status:** Accepted

## Context
Det måtte avklares hvordan `Results` etableres i flyten.

## Decision
`Results` opprettes automatisk når én eller flere participants gir rating.
Mens arrangement er `Started`, oppdateres resultater fortløpende når ratings legges til eller endres.
Ved overgang til `Completed` fryses resultatene.

## Consequences
- Ingen separat manuell oppretting av resultater er nødvendig.
- Krever deterministisk trigger i rating-flyt (første vellykkede rating).
- Resultatberegning blir en kontinuerlig read-model under `Started`, og immutable snapshot etter `Completed`.
