# 0039. Scalar documents all API endpoints

**Status:** Accepted

## Context
Teamet trenger én samlet API-dokumentasjonsside i Scalar som dekker hele API-overflaten, inkludert interne/admin/debug-endepunkter.

## Decision
Scalar-dokumentasjonen skal inkludere alle versjonerte API-endepunkter.
Interne/admin/debug-endepunkter skal grupperes tydelig i en egen seksjon i dokumentasjonen.

## Consequences
- Utviklere og drift får full oversikt over tilgjengelige endepunkter i ett sted.
- Interne ruter blir enklere å finne uten å blande dem med public-forbrukerruter.
- Dokumentasjonsoppsettet må vedlikeholde tydelig tagging/gruppering av interne ruter.
