# 0027. Server generates identifiers for Beer, Rating and Result

**Status:** Accepted

## Context
ID-generering måtte standardiseres for å unngå kollisjoner, replay-problemer og inkonsistent klientlogikk.

## Decision
Server genererer `Id` for `Beer`, `Rating` og `Result`.

## Consequences
- Konsistent identitetshåndtering på tvers av klienter.
- Reduserer risiko for konflikt ved klient-retries.
- Krever at create-kontrakter skiller tydelig mellom request-modeller (uten `Id`) og response-modeller (med `Id`).
