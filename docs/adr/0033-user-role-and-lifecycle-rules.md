# 0033. User role and lifecycle rules

**Status:** Accepted

## Context
User-endepunkter må håndheve klare regler for rolle, identitet og tilgang for å unngå sikkerhets- og dataproblemer.

## Decision
- En bruker har nøyaktig én rolle: `Admin` eller `User`.
- `Email` er globalt unik (case-insensitiv), konflikt gir `409`.
- `IsActive=false` blokkerer tilgang umiddelbart.
- Kun eksisterende `Admin` kan opprette nye `Admin`-brukere.

## Consequences
- Krever unik indeks på normalisert e-post.
- Krever autorisasjonsregel i create-endepunkt for admin-opprettelse.
- Krever sentral policy som avviser inaktive brukere ved autentisering/autorisering.
