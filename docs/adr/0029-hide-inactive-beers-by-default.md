# 0029. Hide inactive beers by default in list endpoints

**Status:** Accepted

## Context
Ved full-liste-endepunkter måtte synlighet av inaktive beers avklares.

## Decision
- `Inactive` beers skjules som default.
- Admin kan hente dem eksplisitt med `includeInactive=true`.

## Consequences
- Vanlige klienter får renere katalog uten utgåtte elementer.
- Admin beholder operativ innsikt i inaktive data.
- Endepunkt må håndheve rollekrav når `includeInactive=true` brukes.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
- Kataloglesing kan caches i Redis når målinger viser gevinst og cache-invalidering håndteres korrekt ved katalogendringer.
