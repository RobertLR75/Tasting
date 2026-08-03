# Domain Context

## User
- **Definition:** Person i systemet med identitet og tilgangsnivå.
- **Attributes:** `Email`, `FirstName`, `LastName`, `IsActive`, `Role`.

## Role
- **Definition:** Autorisasjonsnivå for bruker.
- **Allowed values:** `Admin` eller `User` (nøyaktig én rolle per bruker).

## Active user
- **Definition:** Bruker med `IsActive=true` som kan autentiseres og utføre tillatte operasjoner.

## Inactive user
- **Definition:** Bruker med `IsActive=false`; tilgang blokkeres umiddelbart.

## Email identity
- **Definition:** Primær unik identifikator for bruker på tvers av systemet.
- **Invariant:** Case-insensitiv global unikhet.
