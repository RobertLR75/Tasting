# 0040. Protect internal API docs outside development

**Status:** Accepted

## Context
Interne/admin/debug-endepunkter skal dokumenteres i Scalar, men disse detaljene skal ikke være fritt tilgjengelige utenfor lokal utvikling.

## Decision
Tilgang til intern/admin/debug-dokumentasjon i Scalar skal kreve autentisering utenfor lokal utvikling.
I development beholdes enkel tilgang for lokal utviklingsflyt.

## Consequences
- Reduserer risiko for utilsiktet eksponering av interne kontrakter i delte miljøer.
- Krever eksplisitt sikkerhetskonfigurasjon rundt dokumentasjonsendepunkt i staging/production.
- Beholder lav friksjon for lokal utvikling.
