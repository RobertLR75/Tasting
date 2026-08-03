# 0002. Participant references User entity

**Status:** Proposed

## Context
Domenet trenger både `User` og `Participant` i arrangement. Det var uklart om participant-data skulle kopieres eller referere direkte til bruker.

## Decision
`Participant` i arrangement representeres som referanse til `User` (via `UserId`) i stedet for snapshot av navn/e-post.

## Consequences
- Én sannhet for brukerdata (navn/e-post) ligger i `User`.
- Visning av participant-detaljer krever join/oppslag mot `User`.
- Krever tydelig håndtering av hva som skjer hvis en bruker deaktiveres/slettes.
