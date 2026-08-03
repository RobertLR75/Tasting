# 0021. Admin manages catalog; participants submit ratings

**Status:** Accepted

## Context
Rolleansvar for katalogforvaltning versus ratingflyt måtte tydeliggjøres.

## Decision
- Kun admin kan opprette/endre `Beer`, `BeerStyle`, `BeerType` og `Brewery`.
- Participants kan lese katalogen og sende/endre ratings innen gyldig rating-vindu.

## Consequences
- Endepunkter må bruke tydelige rollekrav (`Admin` vs `Participant`).
- Reduserer risiko for uautoriserte katalogendringer under aktive arrangement.
- Krever entydig autentisering/autorisasjonsmapping mellom brukerroller.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
