# 0030. Optimistic concurrency on rating updates

**Status:** Accepted

## Context
Samtidige writes på samme rating kan ellers gi siste-write-vinner uten kontroll og skjulte overskrivinger.

## Decision
Rating-dokumenter skal bruke optimistic concurrency/version-felt.
Tapende concurrent update returnerer `409 Conflict`.

## Consequences
- Forutsigbar konfliktatferd ved parallelle rating-endringer.
- Klient må håndtere refresh/retry-flyt ved `409`.
- Krever versjonsfelt og atomisk oppdateringsmønster i MongoDB.
